#include "process.h"

#include <Windows.h>
#include <Psapi.h>

#include <fmt/xchar.h>

#include "error.h"

BeDbgApi::Type::sys_handle_t BeDbgApi::Process::AttachProcess(int pid)
{
    if (const auto handle = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
        handle != nullptr)
    {
        return handle;
    }
    *Error::GetInnerError() = Error::Error{Error::ExceptionModule::SYSTEM, GetLastError(), L"OpenProcess"};
    return nullptr;
}

std::uint32_t BeDbgApi::Process::StartProcess(const wchar_t* filename, const wchar_t* command, wchar_t* environment,
                                              const wchar_t* workingDirectory)
{
    wchar_t cmdBuffer[MAX_PATH];
    STARTUPINFOW startupInfo{.cb = sizeof(STARTUPINFOW)};
    PROCESS_INFORMATION processInfo;
    fmt::format_to_n(cmdBuffer, MAX_PATH, L"{}{}", command, '\0');
    const auto success = CreateProcessW(filename, cmdBuffer, nullptr, nullptr, false,
                                        CREATE_UNICODE_ENVIRONMENT | CREATE_NEW_CONSOLE | CREATE_NEW_PROCESS_GROUP |
                                        DEBUG_ONLY_THIS_PROCESS | CREATE_DEFAULT_ERROR_MODE,
                                        environment, workingDirectory, &startupInfo,
                                        &processInfo);
    if (!success)
    {
        *Error::GetInnerError() = Error::Error{Error::ExceptionModule::SYSTEM, GetLastError(), L"CreateProcessW"};
        return 0;
    }
    const auto pid = GetProcessId(processInfo.hProcess);

    CloseHandle(processInfo.hProcess);
    CloseHandle(processInfo.hThread);
    return pid;
}

bool BeDbgApi::Process::DetachProcess(Type::sys_handle_t handle)
{
    const auto success = CloseHandle(handle);
    if (!success)
    {
        *Error::GetInnerError() = Error::Error{Error::ExceptionModule::SYSTEM, GetLastError(), L"CloseHandle"};
    }
    return success;
}

bool BeDbgApi::Process::IsAttachableProcess(int pid)
{
    if (const auto handle = AttachProcess(pid); handle != nullptr)
    {
        return DetachProcess(handle);
    }
    return false;
}

bool BeDbgApi::Process::QueryProcessModules(const Type::sys_handle_t handle,
                                            _Out_writes_(sizeof(ProcessModuleInformation)* count)
                                            ProcessModuleInformation
                                            modules[], const size_t count, size_t* usedCount)
{
    HMODULE moduleHandles[1024];
    DWORD sizeUsed = 0;
    if (K32EnumProcessModulesEx(handle, moduleHandles, 1024u * sizeof(HMODULE), &sizeUsed, LIST_MODULES_ALL) == 0)
    {
        *Error::GetInnerError() = Error::Error{Error::ExceptionModule::SYSTEM, GetLastError(), L"EnumProcessModulesEx"};
        return false;
    }
    const auto moduleCount = sizeUsed / sizeof(HMODULE);
    MODULEINFO moduleInfo;
    size_t used = 0;
    for (auto i = 0ull; i < moduleCount; ++i)
    {
        if (count == i)
        {
            break;
        }
        if (K32GetModuleInformation(handle, moduleHandles[i], &moduleInfo, sizeof(MODULEINFO)))
        {
            used++;
            const auto size = K32GetModuleBaseNameW(handle, moduleHandles[i], modules[i].name, BUFFER_SIZE);
            modules[i].name[size < BUFFER_SIZE ? size : BUFFER_SIZE - 1] = L'\0';
            modules[i].entry = reinterpret_cast<std::uint64_t>(moduleInfo.EntryPoint);
            modules[i].imageBase = reinterpret_cast<std::uint64_t>(moduleInfo.lpBaseOfDll);
            modules[i].size = moduleInfo.SizeOfImage;
        }
    }
    *usedCount = used;
    return true;
}

size_t _Success_(return > 0) BeDbgApi::Process::QueryProcessMemoryInfos(
    const Type::sys_handle_t handle,
    _Out_writes_(sizeof(ProcessMemoryBlockInformation)* count)
    ProcessMemoryBlockInformation infos[], const size_t count)
{
    size_t i = 0;
    SYSTEM_INFO sysInfo;
    GetSystemInfo(&sysInfo);
    auto p = static_cast<char*>(sysInfo.lpMinimumApplicationAddress);
    MEMORY_BASIC_INFORMATION memInfo;
    while (p < sysInfo.lpMaximumApplicationAddress && i < count)
    {
        if (const auto size = VirtualQueryEx(
                handle,
                p,
                &memInfo,
                sizeof(MEMORY_BASIC_INFORMATION)
            ); size != sizeof(MEMORY_BASIC_INFORMATION)
        )
        {
            break;
        }


        infos[i] = {
            .baseAddress = reinterpret_cast<std::uint64_t>(memInfo.BaseAddress),
            .allocAddress = reinterpret_cast<std::uint64_t>(memInfo.AllocationBase),
            .size = memInfo.RegionSize,
            .protectionFlags = memInfo.Protect,
            .initialProtectionFlags = memInfo.AllocationProtect,
            .state = memInfo.State,
            .type = memInfo.Type,
            .info = L""
        };
        if (memInfo.Type == MEM_MAPPED)
        {
            K32GetMappedFileNameW(handle, memInfo.AllocationBase, infos[i].info, BUFFER_SIZE);
        }
        i++;

        p += memInfo.RegionSize;
    }
    return i;
}

BeDbgApi::Type::sys_handle_t BeDbgApi::Process::CopyProcessHandle(Type::sys_handle_t handle)
{
    Type::sys_handle_t result;
    if (DuplicateHandle(handle, handle, handle, &result, 0, true, DUPLICATE_SAME_ACCESS))
    {
        return result;
    }
    *Error::GetInnerError() = Error::Error{Error::ExceptionModule::SYSTEM, GetLastError(), L"DuplicateHandle"};

    return nullptr;
}

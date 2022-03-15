#include "process.h"

#include <Windows.h>
#include <Psapi.h>

#include <fmt/xchar.h>

#include "error.h"

BeDbgApi::Type::handle_t BeDbgApi::Process::AttachProcess(int pid)
{
    if (const auto handle = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
        handle != nullptr)
    {
        return handle;
    }
    *Error::GetInnerError() = Error::Error{Error::ExceptionModule::SYSTEM, GetLastError(), L"OpenProcess"};
    return nullptr;
}

int BeDbgApi::Process::StartProcess(const wchar_t* filename, const wchar_t* command, wchar_t* environment,
                                    const wchar_t* workingDirectory)
{
    wchar_t cmdBuffer[MAX_PATH];
    STARTUPINFOW startupInfo;
    startupInfo.cb = sizeof(STARTUPINFOW);
    PROCESS_INFORMATION processInfo;
    fmt::format_to_n(cmdBuffer, MAX_PATH, L"{}{}", command, '\0');
    const auto success = CreateProcessW(filename, cmdBuffer, nullptr, nullptr, false,
                                        CREATE_UNICODE_ENVIRONMENT | CREATE_NEW_CONSOLE | CREATE_NEW_PROCESS_GROUP |
                                        CREATE_DEFAULT_ERROR_MODE, environment, workingDirectory, &startupInfo,
                                        &processInfo);
    if (!success)
    {
        *Error::GetInnerError() = Error::Error{Error::ExceptionModule::SYSTEM, GetLastError(), L"CreateProcessW"};
        return 0;
    }
    auto pid = GetProcessId(processInfo.hProcess);
    CloseHandle(processInfo.hProcess);
    CloseHandle(processInfo.hThread);
    return pid;
}

bool BeDbgApi::Process::DetachProcess(Type::handle_t handle)
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

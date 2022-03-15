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

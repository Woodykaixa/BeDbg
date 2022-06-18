#include "debug.h"

#include <fmt/xchar.h>

#include "error.h"

using namespace BeDbgApi::Debug;
constexpr static std::uint8_t INT3 = 0xCC;

BeDbgApi::Type::handle_t<DebugLoopCallbacks> BeDbgApi::Debug::CreateDebugLoopCallbacks()
{
    const auto callbacks = new DebugLoopCallbacks;
    memset(callbacks, 0, sizeof(DebugLoopCallbacks));
    return callbacks;
}

void BeDbgApi::Debug::SetDebugLoopCallback(const Type::handle_t<DebugLoopCallbacks> callbacks, const int eventId,
                                           const DebugEventCallback<> callback)
{
    if (!callbacks)
    {
        auto* error = Error::GetInnerError();
        error->exceptionModule = Error::ExceptionModule::DEBUG;
        Error::SetApiParamError(1);
        error->message = fmt::format(L"[SetDebugLoopCallback] Unknown eventId {}", eventId);
        return;
    }

    if (eventId < EXCEPTION_DEBUG_EVENT || eventId > RIP_EVENT)
    {
        return;
    }
    const auto cbArr = static_cast<DebugEventCallback<>*>(static_cast<void*>(callbacks));
    cbArr[eventId - 1] = callback;
}

void BeDbgApi::Debug::DestroyDebugLoopCallbacks(const Type::handle_t<DebugLoopCallbacks> cb)
{
    delete cb;
}

DebugContinueStatus Internal::dispatchDebugEvent(const DEBUG_EVENT* event,
                                                 const Type::handle_t<DebugLoopCallbacks> callbacks)
{
    const auto eventCode = event->dwDebugEventCode;

    if (eventCode < EXCEPTION_DEBUG_EVENT || eventCode > RIP_EVENT)
    {
        *Error::GetInnerError() = {
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = fmt::format(L"[dispatchDebugEvent] Unknown event code {}", eventCode),
        };
        return DebugContinueStatus::NotHandled;
    }
    const auto cbArr = static_cast<DebugEventCallback<>*>(static_cast<void*>(callbacks));
    const auto cb = cbArr[eventCode - 1]; 
    if (cb == nullptr)
    {
        return DebugContinueStatus::AutoContinue;
    }
    return cb(event->dwProcessId, event->dwThreadId, &event->u);
}

DebugContinueStatus BeDbgApi::Debug::DebugLoopWaitEvent(const Type::handle_t<DebugLoopCallbacks> callbacks)
{
    DEBUG_EVENT event;
    if (!WaitForDebugEvent(&event, INFINITE))
    {
        *Error::GetInnerError() = {
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = L"WaitForDebugEvent",
        };
        return DebugContinueStatus::NotHandled;
    }
    const auto result = Internal::dispatchDebugEvent(&event, callbacks);
    if (result == DebugContinueStatus::WaitForExplicitContinue)
    {
        return result;
    }
    const auto debugStatusCode = result == DebugContinueStatus::AutoContinue ? DBG_CONTINUE : DBG_EXCEPTION_NOT_HANDLED;
    ContinueDebugEvent(event.dwProcessId, event.dwThreadId, debugStatusCode);
    return result;
}

_Success_(return)

bool BeDbgApi::Debug::SetBreakpoint(const Type::sys_handle_t processHandle, const std::uint64_t address,
                                    _Out_writes_(1)std::uint8_t* originalCode)
{
    std::uint8_t buffer{};

    auto success = ReadProcessMemory(processHandle, reinterpret_cast<void*>(address), &buffer, 1, nullptr);
    if (!success)
    {
        *Error::GetInnerError() = Error::Error{
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = L"ReadProcessMemory"
        };
        return false;
    }
    originalCode[0] = buffer;
    success = WriteProcessMemory(processHandle, reinterpret_cast<void*>(address), &INT3, 1, nullptr);
    if (!success)
    {
        *Error::GetInnerError() = Error::Error{
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = L"WriteProcessMemory"
        };
        return false;
    }
    return true;
}

bool BeDbgApi::Debug::RemoveBreakpoint(const Type::sys_handle_t processHandle, const std::uint64_t address,
                                       const std::uint8_t originalCode)
{
    if (const auto success = WriteProcessMemory(processHandle, reinterpret_cast<void*>(address), &originalCode, 1,
                                                nullptr); !success)
    {
        *Error::GetInnerError() = Error::Error{
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = L"WriteProcessMemory"
        };
        return false;
    }
    return true;
}

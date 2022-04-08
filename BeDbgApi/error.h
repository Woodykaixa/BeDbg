#pragma once
#include <string>

#include <cstdint>

#include "defines.h"

namespace BeDbgApi::Error
{
    enum class ExceptionModule:std::uint32_t
    {
        OK_NO_ERROR,
        SYSTEM,
        DEBUG,
        UNKNOWN
    };

    struct Error
    {
        ExceptionModule exceptionModule{ExceptionModule::OK_NO_ERROR};
        std::uint32_t code{};
        std::wstring message;

        std::uint64_t errorCode();
    };

    BEDBG_API Error* GetInnerError();
    BEDBG_API bool HasError();
    BEDBG_API void ClearError();
    BEDBG_API std::uint64_t GetError();
    BEDBG_API const wchar_t* GetErrorMessage();
}

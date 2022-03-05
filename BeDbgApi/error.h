#pragma once
#include "defines.h"
#include <string>
#include <optional>

namespace BeDbgApi::Error
{
    enum class ExceptionModule:std::uint32_t
    {
        NO_ERROR,
        SYSTEM,
        ASM,
        UNKNOWN
    };

    struct Error
    {
        ExceptionModule module{ExceptionModule::NO_ERROR};
        std::uint32_t code{};
        std::string message;

        std::uint64_t errorCode();
    };

    inline std::optional<Error> innerError;

    BEDBG_API bool HasError();
    BEDBG_API void ClearError();
    BEDBG_API std::uint64_t GetError();
    BEDBG_API const char* GetErrorMessage();
}

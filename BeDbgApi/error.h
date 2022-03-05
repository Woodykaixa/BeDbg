#pragma once
#include "defines.h"
#include <string>
#include <optional>

namespace BeDbgApi::Error
{
    struct Error
    {
        Type::Module module;
        std::uint64_t code{};
        std::string message;
    };

    inline std::optional<Error> innerError;

    BEDBG_API bool HasError();
    BEDBG_API void ClearError();
    BEDBG_API Error GetError();
}

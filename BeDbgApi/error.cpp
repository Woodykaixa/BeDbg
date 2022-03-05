#include "error.h"

std::uint64_t BeDbgApi::Error::Error::errorCode()
{
    return static_cast<std::uint64_t>(module) << 32u | code;
}

bool BeDbgApi::Error::HasError()
{
    return innerError.has_value();
}

void BeDbgApi::Error::ClearError()
{
    innerError = std::nullopt;
}

std::uint64_t BeDbgApi::Error::GetError()
{
    if (innerError.has_value())
    {
        return innerError.value().errorCode();
    }
    return 0;
}

const char* BeDbgApi::Error::GetErrorMessage()
{
    if (innerError.has_value())
    {
        return innerError.value().message.c_str();
    }
    return nullptr;
}

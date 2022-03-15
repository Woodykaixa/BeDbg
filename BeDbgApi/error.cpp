#include "error.h"
#include <fmt/xchar.h>

std::uint64_t BeDbgApi::Error::Error::errorCode()
{
    return (static_cast<std::uint64_t>(exceptionModule) << 32u) | code;
}

BeDbgApi::Error::Error* BeDbgApi::Error::GetInnerError()
{
    static Error innerError = {ExceptionModule::OK_NO_ERROR, 0, L""};
    return &innerError;
}

bool BeDbgApi::Error::HasError()
{
    return GetInnerError()->exceptionModule != ExceptionModule::OK_NO_ERROR;
}

void BeDbgApi::Error::ClearError()
{
    *GetInnerError() = Error{ExceptionModule::OK_NO_ERROR, 0, L""};
}

std::uint64_t BeDbgApi::Error::GetError()
{
    return GetInnerError()->errorCode();
}

const wchar_t* BeDbgApi::Error::GetErrorMessage()
{
    return GetInnerError()->message.c_str();
}

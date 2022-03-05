#include "error.h"

bool BeDbgApi::Error::HasError()
{
    return innerError.has_value();
}

void BeDbgApi::Error::ClearError()
{
    innerError = std::nullopt;
}

BeDbgApi::Error::Error BeDbgApi::Error::GetError()
{
    return innerError.value_or(Error{Type::Module::UNKNOWN, 0, ""});
}

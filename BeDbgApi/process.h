#pragma once
#include "defines.h"

namespace BeDbgApi::Process
{
    EXPORT Type::handle_t StartProcess(const wchar_t* executable, const wchar_t* param);
}

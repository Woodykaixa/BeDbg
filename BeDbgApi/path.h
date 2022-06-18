#pragma once
#include "defines.h"

#include <cstdint>

#include <Windows.h>


namespace BeDbgApi::Path
{
    BEDBG_API bool GetPathFromFileHandle(Type::sys_handle_t fileHandle, _Out_writes_(size) wchar_t* pathBuffer,
                                         std::uint32_t size);
}

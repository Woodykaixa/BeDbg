#pragma once
#include "defines.h"
#include <Zydis/Decoder.h>

namespace BeDbgApi::Asm
{
    BEDBG_API Type::handle_t CreateDecoder(ZydisMachineMode machineMode, ZydisAddressWidth addressWidth);
    BEDBG_API void DestroyDecoder(Type::handle_t decoder);
}

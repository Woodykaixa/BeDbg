#pragma once
#include "defines.h"
#include <Zydis/Decoder.h>
#include <cstdint>

namespace BeDbgApi::Asm
{
    BEDBG_API Type::handle_t CreateDecoder(ZydisMachineMode machineMode, ZydisAddressWidth addressWidth);
    BEDBG_API void DestroyDecoder(Type::handle_t decoder);

    BEDBG_API Type::handle_t DecodeInstruction(Type::handle_t decoder, const std::uint8_t* buffer, size_t size);
    BEDBG_API void DestroyInstruction(Type::handle_t instruction);

}

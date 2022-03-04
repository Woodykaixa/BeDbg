#pragma once
#include "defines.h"
#include <Zydis/Decoder.h>

namespace BeDbgApi::Asm
{
    class Decoder
    {
    private:
        ZydisDecoder _decoder;

    public:
        Decoder(ZydisMachineMode machineMode, ZydisAddressWidth addressWidth);
    };
}

#include "asm.h"
#include <Zydis/Decoder.h>

using namespace BeDbgApi;

Asm::Decoder::Decoder(ZydisMachineMode machineMode, ZydisAddressWidth addressWidth)
{
    const auto status = ZydisDecoderInit(&_decoder, machineMode, addressWidth);
}

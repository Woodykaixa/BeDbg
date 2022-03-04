#include "asm.h"
#include <fmt/core.h>

using namespace BeDbgApi;

Asm::Decoder::Decoder(ZydisMachineMode machineMode, ZydisAddressWidth addressWidth)
{
    fmt::print("{} {}\n", machineMode, addressWidth);
}

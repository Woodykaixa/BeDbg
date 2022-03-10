#include "asm.h"
#include "error.h"
using namespace BeDbgApi;

Type::handle_t Asm::CreateDecoder(ZydisMachineMode machineMode, ZydisAddressWidth addressWidth)
{
    auto* decoder = new ZydisDecoder;
    const auto status = ZydisDecoderInit(decoder, machineMode, addressWidth);
    if (ZYAN_SUCCESS(status))
    {
        return decoder;
    }
    Error::innerError = {Error::ExceptionModule::ASM, status, L"see zydis doc"};
    delete decoder;
    return nullptr;
}

void Asm::DestroyDecoder(const Type::handle_t decoder)
{
    delete static_cast<ZydisDecoder*>(decoder);
}

Type::handle_t Asm::DecodeInstruction(const Type::handle_t decoder, const std::uint8_t* buffer, const size_t size)
{
    auto* instruction = new ZydisDecodedInstruction;
    const auto status = ZydisDecoderDecodeBuffer(static_cast<ZydisDecoder*>(decoder), buffer, size, instruction);
    if (ZYAN_SUCCESS(status))
    {
        return instruction;
    }
    Error::innerError = {Error::ExceptionModule::ASM, status, L"see zydis doc"};
    delete instruction;
    return nullptr;
}

void Asm::DestroyInstruction(const Type::handle_t instruction)
{
    delete static_cast<ZydisDecoder*>(instruction);
}

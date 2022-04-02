#include "asm.h"

#include <fmt/xchar.h>

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
    *Error::GetInnerError() = {Error::ExceptionModule::ASM, status, L"see zydis doc"};
    delete decoder;
    return nullptr;
}

void Asm::DestroyDecoder(const Type::handle_t decoder)
{
    delete static_cast<ZydisDecoder*>(decoder);
}

Type::handle_t Asm::CreateFormatter(ZydisFormatterStyle formatterStyle)
{
    auto* formatter = new ZydisFormatter;
    const auto status = ZydisFormatterInit(formatter, formatterStyle);
    if (ZYAN_SUCCESS(status))
    {
        return formatter;
    }
    *Error::GetInnerError() = {
        Error::ExceptionModule::ASM, status,
        fmt::format(L"ZydisFormatterInit failed. formatterStyle = {}", formatterStyle)
    };
    delete formatter;
    return nullptr;
}

void Asm::DestroyFormatter(Type::handle_t formatter)
{
    delete static_cast<ZydisFormatter*>(formatter);
}

Type::handle_t Asm::DecodeInstruction(const Type::handle_t decoder, const std::uint8_t* buffer, const size_t size)
{
    auto* instruction = new ZydisDecodedInstruction;
    const auto status = ZydisDecoderDecodeBuffer(static_cast<ZydisDecoder*>(decoder), buffer, size, instruction);
    if (ZYAN_SUCCESS(status))
    {
        return instruction;
    }
    *Error::GetInnerError() = {Error::ExceptionModule::ASM, status, L"see zydis doc"};
    delete instruction;
    return nullptr;
}

void Asm::DestroyInstruction(const Type::handle_t instruction)
{
    delete static_cast<ZydisDecoder*>(instruction);
}

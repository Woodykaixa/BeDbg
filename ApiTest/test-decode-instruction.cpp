#include "pch.h"
#include <array>
#include <asm.h>
#include <error.h>
#include <tuple>
using namespace BeDbgApi;

class TestDecodeInstruction : public testing::Test
{
protected:
    Type::handle_t decoder;
    ZydisDecodedInstruction* instruction;

    void SetUp() override
    {
        decoder = nullptr;
        instruction = nullptr;
        Error::ClearError();
    }

    void TearDown() override
    {
        Asm::DestroyDecoder(decoder);
        Asm::DestroyInstruction(instruction);
    }
};

TEST_F(TestDecodeInstruction, ValidInstruction)
{
    decoder = Asm::CreateDecoder(ZYDIS_MACHINE_MODE_LONG_64, ZYDIS_ADDRESS_WIDTH_64);
    constexpr std::uint8_t data[]{0x51};
    instruction = static_cast<ZydisDecodedInstruction*>(Asm::DecodeInstruction(decoder, data, 1));
    EXPECT_EQ(Error::HasError(), false);
    EXPECT_EQ(instruction->length, 1);
    EXPECT_EQ(instruction->mnemonic, ZYDIS_MNEMONIC_PUSH);
    EXPECT_EQ(instruction->stack_width, 64);
    EXPECT_EQ(instruction->operands[0].reg.value, ZYDIS_REGISTER_RCX);
}

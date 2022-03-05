#include "pch.h"
#include <asm.h>
#include <error.h>
#include <tuple>

using namespace BeDbgApi;

class TestCreateDecoder : public testing::Test
{
protected:
    std::tuple<std::uint32_t, Error::ExceptionModule> parseErrorCode(std::uint64_t error)
    {
        const std::uint32_t code = error & 0xFFFFFFFF;
        const auto module = static_cast<Error::ExceptionModule>(error >> 32);
        return std::make_tuple(code, module);
    }

    void SetUp() override
    {
        Error::ClearError();
    }
};

TEST_F(TestCreateDecoder, ValidParam)
{
    const auto decoder = Asm::CreateDecoder(ZYDIS_MACHINE_MODE_LONG_64, ZYDIS_ADDRESS_WIDTH_64);
    EXPECT_NE(decoder, nullptr);
    EXPECT_EQ(Error::HasError(), false);
    Asm::DestroyDecoder(decoder);
}


TEST_F(TestCreateDecoder, InvalidAddressWidth)
{
    const auto decoder = Asm::CreateDecoder(ZYDIS_MACHINE_MODE_LONG_64, static_cast<ZydisAddressWidth>(100));
    EXPECT_EQ(decoder, nullptr);
    EXPECT_EQ(Error::HasError(), true);
    const auto [code, module] = this->parseErrorCode(Error::GetError());
    EXPECT_EQ(module, Error::ExceptionModule::ASM);
    EXPECT_EQ(code, ZYAN_STATUS_INVALID_ARGUMENT);
}

TEST_F(TestCreateDecoder, InvalidMachineMode)
{
    const auto decoder = Asm::CreateDecoder(static_cast<ZydisMachineMode>(100), ZYDIS_ADDRESS_WIDTH_64);
    EXPECT_EQ(decoder, nullptr);
    EXPECT_EQ(Error::HasError(), true);
    const auto [code, module] = this->parseErrorCode(Error::GetError());
    EXPECT_EQ(module, Error::ExceptionModule::ASM);
    EXPECT_EQ(code, ZYAN_STATUS_INVALID_ARGUMENT);
}


TEST_F(TestCreateDecoder, InvalidParam)
{
    const auto decoder = Asm::CreateDecoder(static_cast<ZydisMachineMode>(100), static_cast<ZydisAddressWidth>(100));
    EXPECT_EQ(decoder, nullptr);
    EXPECT_EQ(Error::HasError(), true);
    const auto [code, module] = this->parseErrorCode(Error::GetError());
    EXPECT_EQ(module, Error::ExceptionModule::ASM);
    EXPECT_EQ(code, ZYAN_STATUS_INVALID_ARGUMENT);
}

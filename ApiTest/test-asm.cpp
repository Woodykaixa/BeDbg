#include "pch.h"
#include <asm.h>
#include <error.h>

using namespace BeDbgApi;
TEST(CreateDecoder, ValidParam)
{
    const auto decoder = Asm::CreateDecoder(ZYDIS_MACHINE_MODE_LONG_64, ZYDIS_ADDRESS_WIDTH_64);
    EXPECT_NE(decoder, nullptr);
    Asm::DestroyDecoder(decoder);
}

TEST(CreateDecoder, InvalidParam)
{
    const auto decoder1 = Asm::CreateDecoder(ZYDIS_MACHINE_MODE_LONG_64, static_cast<ZydisAddressWidth>(100));
    EXPECT_EQ(decoder1, nullptr);
    EXPECT_EQ(Error::HasError(), true);
    const auto err = Error::GetError();
    EXPECT_EQ(err.module, Type::Module::ASM);
    EXPECT_NE(err.code, ZYAN_STATUS_SUCCESS);

    const auto decoder2 = Asm::CreateDecoder(static_cast<ZydisMachineMode>(100), ZYDIS_ADDRESS_WIDTH_64);
    EXPECT_EQ(decoder2, nullptr);

    const auto decoder3 = Asm::CreateDecoder(static_cast<ZydisMachineMode>(100), static_cast<ZydisAddressWidth>(100));
    EXPECT_EQ(decoder3, nullptr);
}

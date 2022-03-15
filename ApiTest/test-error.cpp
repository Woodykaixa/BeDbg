#include "pch.h"
#include "error.h"

using namespace BeDbgApi;

void raiseError()
{
    *Error::GetInnerError() = Error::Error{Error::ExceptionModule::SYSTEM, 1u, L"Test"};
}

TEST(TestError, ClearError)
{
    Error::ClearError();
    EXPECT_EQ(Error::HasError(), false);
}

TEST(TestError, InitialValue)
{
    EXPECT_EQ(Error::HasError(), false);
}

TEST(TestError, DetectError)
{
    raiseError();
    const auto& [exceptionModule, code, message] = *Error::GetInnerError();
    EXPECT_EQ(code, 1u);
    EXPECT_EQ(exceptionModule, Error::ExceptionModule::SYSTEM);
    EXPECT_EQ(message, L"Test");
    EXPECT_EQ(Error::GetError(), 0x0000000100000001); // SYSTEM | 1
    EXPECT_EQ(Error::HasError(), true);
    EXPECT_STREQ(Error::GetErrorMessage(), L"Test");
}

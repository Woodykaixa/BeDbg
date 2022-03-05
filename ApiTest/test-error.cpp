#include "pch.h"
#include "error.h"

using namespace BeDbgApi;
TEST(TestError, ClearError)
{
    Error::ClearError();
    EXPECT_EQ(Error::HasError(), false);
}

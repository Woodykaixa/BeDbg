#include "pch.h"
#include <api.h>

TEST(TestSumApi, TestPositiveAdd)
{
    const auto result = sum(1, 2);
    EXPECT_EQ(result, 3);
}

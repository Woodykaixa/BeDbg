#include "pch.h"
#include <defines.h>


TEST(TestEnv, TestPlatform)
{
#ifdef WIN32
	EXPECT_EQ(BeDbgApi::Env::PLATFORM, BeDbgApi::Env::Platform::X86);
	EXPECT_TRUE(BeDbgApi::Env::IS_WIN32);
#else
	EXPECT_EQ(BeDbgApi::Env::PLATFORM, BeDbgApi::Env::Platform::AMD64);
	EXPECT_FALSE(BeDbgApi::Env::IS_WIN32);
#endif
}

TEST(TestEnv, TestConfiguration)
{
	EXPECT_EQ(BeDbgApi::Env::CONFIGURATION, BeDbgApi::Env::Configuration::DEBUG);
	EXPECT_TRUE(BeDbgApi::Env::IS_DEBUG);
}

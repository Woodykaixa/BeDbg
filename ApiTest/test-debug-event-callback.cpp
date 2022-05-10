#include "pch.h"

#include <functional>

#include "debug.h"

using namespace BeDbgApi;

struct TestData
{
    int OnException;
    int OnCreateThread;
    int OnCreateProcess;
    int OnExitThread;
    int OnExitProcess;
    int OnLoadDll;
    int OnUnloadDll;
    int OnOutputDebugString;
    int OnRip;
} testData = {0, 0, 0, 0, 0, 0, 0, 0, 0};

class DebugEventCallback : public testing::Test
{
protected:
    Debug::DebugLoopCallbacks* _cb = nullptr;

    void SetUp() override
    {
        _cb = Debug::CreateDebugLoopCallbacks();
        ASSERT_NE(_cb, nullptr);
    }

    void TearDown() override
    {
        DestroyDebugLoopCallbacks(_cb);
    }
};

#define TestCallback(cbName, key, test) \
TEST_F(DebugEventCallback, cbName) \
{ \
    EXPECT_EQ(testData.cbName, 0);\
    SetDebugLoopCallback(_cb, key, [](std::uint32_t pid, std::uint32_t tid, \
                                                            const auto* info) \
    { \
        testData.cbName = test; \
        return Debug::DebugContinueStatus::AutoContinue; \
    }); \
    EXPECT_EQ(_cb->cbName(0, 0, nullptr), Debug::DebugContinueStatus::AutoContinue); \
    EXPECT_EQ(testData.cbName, test); \
    testData.cbName = 0; \
}

TestCallback(OnException, EXCEPTION_DEBUG_EVENT, 1);
TestCallback(OnCreateThread, CREATE_THREAD_DEBUG_EVENT, 14);
TestCallback(OnCreateProcess, CREATE_PROCESS_DEBUG_EVENT, 51);
TestCallback(OnExitThread, EXIT_THREAD_DEBUG_EVENT, 4);
TestCallback(OnExitProcess, EXIT_PROCESS_DEBUG_EVENT, 19);
TestCallback(OnLoadDll, LOAD_DLL_DEBUG_EVENT, 198);
TestCallback(OnUnloadDll, UNLOAD_DLL_DEBUG_EVENT, 10);
TestCallback(OnOutputDebugString, OUTPUT_DEBUG_STRING_EVENT, 114);
TestCallback(OnRip, RIP_EVENT, 514);

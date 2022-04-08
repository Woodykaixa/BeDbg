#include "pch.h"

#include "debug.h"
#include "error.h"


using namespace BeDbgApi;

class DispatchDebugEvent : public testing::Test
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

#define TestDispatch(cbName, key, testPid, testTid) \
TEST_F(DispatchDebugEvent, cbName) \
{ \
    Error::ClearError(); \
    SetDebugLoopCallback(_cb, key, [](std::uint32_t pid, std::uint32_t tid, \
                                                        const auto* info) \
    { \
        EXPECT_EQ(pid, testPid); \
        EXPECT_EQ(tid, testTid); \
        return Debug::DebugContinueStatus::Continue; \
    }); \
    const DEBUG_EVENT event \
    { \
        .dwDebugEventCode = (key), \
        .dwProcessId =  (testPid), \
        .dwThreadId = (testTid), \
        .u = {} \
    }; \
    const auto result = Debug::Internal::dispatchDebugEvent(&event, _cb); \
    EXPECT_EQ(result, Debug::DebugContinueStatus::Continue); \
    EXPECT_FALSE(Error::HasError());\
}

TestDispatch(OnException, EXCEPTION_DEBUG_EVENT, 5, 16); // Aoyama Nagisa
TestDispatch(OnCreateThread, CREATE_THREAD_DEBUG_EVENT, 3, 5); // Yano Hinaki
TestDispatch(OnCreateProcess, CREATE_PROCESS_DEBUG_EVENT, 2, 27); // IPhone Shower
TestDispatch(OnExitThread, EXIT_THREAD_DEBUG_EVENT, 5, 24); // Uchida Shu
TestDispatch(OnExitProcess, EXIT_PROCESS_DEBUG_EVENT, 5, 2); //Onishi Aguri
TestDispatch(OnLoadDll, LOAD_DLL_DEBUG_EVENT, 9, 30); // Date Sayuri
TestDispatch(OnUnloadDll, UNLOAD_DLL_DEBUG_EVENT, 12, 22); // Kutsunoki Tomori
TestDispatch(OnOutputDebugString, OUTPUT_DEBUG_STRING_EVENT, 1, 9); // Liyuu
TestDispatch(OnRip, RIP_EVENT, 10, 16); // Kito Akari


TEST_F(DispatchDebugEvent, SmallInvalidKey)
{
    const DEBUG_EVENT event
    {
        .dwDebugEventCode = 0,
        .dwProcessId = 0,
        .dwThreadId = 0,
        .u = {}
    };
    const auto result = Debug::Internal::dispatchDebugEvent(&event, _cb);
    EXPECT_EQ(result, Debug::DebugContinueStatus::NotHandled);
    const auto error = Error::GetInnerError();
    EXPECT_EQ(error->exceptionModule, Error::ExceptionModule::DEBUG);
    EXPECT_STREQ(error->message.c_str(), L"[dispatchDebugEvent] Unknown event code 0");
}


TEST_F(DispatchDebugEvent, LargeInvalidKey)
{
    const DEBUG_EVENT event
    {
        .dwDebugEventCode = 10,
        .dwProcessId = 0,
        .dwThreadId = 0,
        .u = {}
    };
    const auto result = Debug::Internal::dispatchDebugEvent(&event, _cb);
    EXPECT_EQ(result, Debug::DebugContinueStatus::NotHandled);
    const auto error = Error::GetInnerError();
    EXPECT_EQ(error->exceptionModule, Error::ExceptionModule::DEBUG);
    EXPECT_STREQ(error->message.c_str(), L"[dispatchDebugEvent] Unknown event code 10");
}

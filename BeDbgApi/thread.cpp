#include "thread.h"

#include <Windows.h>

#include <fmt/xchar.h>

#include "error.h"

void BeDbgApi::Thread::GetThreadRegisters(Type::sys_handle_t thread, _Out_ Registers* reg)
{
    CONTEXT ctx;
    ctx.ContextFlags = CONTEXT_ALL;
    GetThreadContext(thread, &ctx);
    reg->rip = ctx.Rip;

    memcpy(&reg->rax, &ctx.Rax, sizeof(std::uint64_t) * 16);

    reg->mxCsr = ctx.MxCsr;

    reg->segCs = ctx.SegCs;
    reg->segDs = ctx.SegDs;
    reg->segEs = ctx.SegEs;
    reg->segFs = ctx.SegFs;
    reg->segGs = ctx.SegGs;
    reg->segSs = ctx.SegSs;
    reg->eFlags = ctx.EFlags;

    memcpy(&reg->dr0, &ctx.Dr0, sizeof(std::uint64_t) * 6);

    reg->fpuTagWord = ctx.FltSave.TagWord;
    reg->fpuStatusWord = ctx.FltSave.StatusWord;
    reg->fpuControlWord = ctx.FltSave.ControlWord;

    memcpy(&reg->st0, &ctx.FltSave.FloatRegisters[0], sizeof(Type::uint128_t));
    memcpy(&reg->st1, &ctx.FltSave.FloatRegisters[1], sizeof(Type::uint128_t));
    memcpy(&reg->st2, &ctx.FltSave.FloatRegisters[2], sizeof(Type::uint128_t));
    memcpy(&reg->st3, &ctx.FltSave.FloatRegisters[3], sizeof(Type::uint128_t));
    memcpy(&reg->st4, &ctx.FltSave.FloatRegisters[4], sizeof(Type::uint128_t));
    memcpy(&reg->st5, &ctx.FltSave.FloatRegisters[5], sizeof(Type::uint128_t));
    memcpy(&reg->st6, &ctx.FltSave.FloatRegisters[6], sizeof(Type::uint128_t));
    memcpy(&reg->st7, &ctx.FltSave.FloatRegisters[7], sizeof(Type::uint128_t));

    memcpy(&reg->xmm, ctx.FltSave.XmmRegisters, sizeof(Type::uint128_t) * 16);
}

std::uint32_t BeDbgApi::Thread::GetThreadContextFlag(const Type::sys_handle_t threadHandle)
{
    CONTEXT context{};
    context.ContextFlags = CONTEXT_CONTROL;
    if (!GetThreadContext(threadHandle, &context))
    {
        *Error::GetInnerError() = Error::Error{
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = fmt::format(L"GetThreadContext return false")
        };
    }
    return context.EFlags;
}

bool BeDbgApi::Thread::SetThreadContextFlag(const Type::sys_handle_t threadHandle, const std::uint32_t flag)
{
    CONTEXT context{};
    context.ContextFlags = CONTEXT_CONTROL;
    if (!GetThreadContext(threadHandle, &context))
    {
        *Error::GetInnerError() = Error::Error{
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = fmt::format(L"GetThreadContext return false")
        };
    }
    context.EFlags = flag;
    const auto result = SetThreadContext(threadHandle, &context);
    if (!result)
    {
        *Error::GetInnerError() = Error::Error{
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = fmt::format(L"SetThreadContext return false")
        };
    }
    return result;
}

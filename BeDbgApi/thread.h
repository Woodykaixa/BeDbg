#pragma once
#include <cstdint>
#include "defines.h"

namespace BeDbgApi::Thread
{
    struct Registers
    {
        std::uint64_t rip;

        std::uint64_t rax;
        std::uint64_t rcx;
        std::uint64_t rdx;
        std::uint64_t rbx;
        std::uint64_t rsp;
        std::uint64_t rbp;
        std::uint64_t rsi;
        std::uint64_t rdi;
        std::uint64_t r8;
        std::uint64_t r9;
        std::uint64_t r10;
        std::uint64_t r11;
        std::uint64_t r12;
        std::uint64_t r13;
        std::uint64_t r14;
        std::uint64_t r15;

        std::uint32_t mxCsr;

        std::uint16_t segCs;
        std::uint16_t segDs;
        std::uint16_t segEs;
        std::uint16_t segFs;
        std::uint16_t segGs;
        std::uint16_t segSs;
        std::uint32_t eFlags;

        std::uint64_t dr0;
        std::uint64_t dr1;
        std::uint64_t dr2;
        std::uint64_t dr3;
        std::uint64_t dr6;
        std::uint64_t dr7;

        std::uint8_t fpuTagWord;
        std::uint16_t fpuStatusWord;
        std::uint16_t fpuControlWord;

        Type::uint128_t st0;
        Type::uint128_t st1;
        Type::uint128_t st2;
        Type::uint128_t st3;
        Type::uint128_t st4;
        Type::uint128_t st5;
        Type::uint128_t st6;
        Type::uint128_t st7;

        Type::uint128_t xmm[16];
    };


    BEDBG_API void GetThreadRegisters(Type::sys_handle_t thread, _Out_ Registers* reg);
}

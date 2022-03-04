# BeDbg Dependencies

## dependencies

- [fmt](https://github.com/fmtlib/fmt): version 8.1.0
- [zydis](https://github.com/zyantific/zydis): version 3.2.1

## Build tool version

CMake 3.21

ninja 1.10.2

### Q: Why use pre-built dependency files

A: It's hard to maintain a build system with package manage system. I've tried CMake/XMake with vcpkg/xrepo, but these solutions causes various problems on CI. Thus I use pre-built binary libraries and simply link them in projects.

### Q: How's these dependencies built

A: Using vcpkg. I manually install both x86 and x64 type of packages.

```shell
# x86
vcpkg install [packages] --triplet=x86-windows-static-md

# x64
vcpkg install [packages] --triplet=x64-windows-static-md
```

vcpkg baseline: `5fcea86177e7c81fa90e721bba2822649dd4b982`

vcpkg version:

```shell
> vcpkg version
< Vcpkg package management program version 2021-12-09-724cea8b33cbf06c645f5095fa29773697da9761
```

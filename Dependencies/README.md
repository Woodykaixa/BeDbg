# BeDbg Dependencies

### Q: Why use pre-built dependency files
A: It's hard to maintain a build system with package manage system. I've tried CMake/XMake with vcpkg/xrepo, but these solutions causes various problems on CI. Thus I use pre-built binary libraries and simply link them in projects.
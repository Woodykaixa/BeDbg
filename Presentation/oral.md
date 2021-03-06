大家好，我毕设的题目是基于 Windows Debug API的动态调试器。我的汇报分为以下几个部分：项目背景、项目设计、项目进度和后续规划。

---

首先要介绍一下项目背景。

调试器是一种可以控制和修改软件的运行流程，以及查看软件运行信息的程序。逆向工程工程师使用它来分析程序行为，以便于识别恶意代码或是挖掘漏洞。调试器的功能很强大，但是现在网上常见的动态调试器或多或少都有一定的门槛，不利于初学者入门逆向工程。

---

因此需要设计一个新的调试器，它要足够简单能够帮助初学者上手，也要有灵活简单的SDK来降低插件编写难度。

---
接下来讲一下架构设计。

项目采用B/S架构，也就是使用浏览器来绘制界面，然后启动一个服务器来进行调试。因为用户界面运行在浏览器中，可以使用 JavaScript 提供插件的SDK，使用js的好处是它语法简单，不需要预先编译就能在执行，还有丰富的生态可以使用。于是最终确定下来关于这个调试器的技术选型。大家可以浅看一下。

项目架构如图所示，分为四个模块。核心 API 封装系统的 Debug API，导出了符合项目需求的 API 接口。服务器调用这些接口，实现一个动态调试器，对外提供调试服务。用户界面运行在浏览器中，负责展示服务器返回的信息，并且在用户做出操作的时候通知服务器进行更新。最后就是JS插件，插件有两个功能，一个是封装了服务器接口，这样调试服务就能以可编程的方式使用，简化一些重复的体力劳动，第二个功能是可以扩展用户界面，提供补充信息。 

项目的运行流程可以用这样一个状态图表示。首先是初始化阶段，程序会加载核心API和一个Web服务器，然后通过服务器启动一个用户界面。当用户选择了一个程序进行调试，程序就会新建一个线程，在新线程中启动一个调试循环，不断监听系统发出的调试事件，并把不同的事件派发到相应的处理分支中执行。在被调试程序加载完成后，就可以同时监听系统事件和用户操作。直到用户发出一个中止调试的指令，退出循环结束调试。

--- 

关于毕设进度，目前核心API已经实现了大约85%，足以支持基本的调试行为。服务器和用户界面都实现了60%，现在可以用调试器调试一个程序，进入调试循环。最后是插件SDK，因为插件依赖于服务器，所以进度稍微落后，只实现了50%。

从上一篇幻灯片的流程图来看，目前程序初始化，启动调试都已经完成了。调试循环也已经支持了处理系统调试事件，以及读取汇编、内存等进程运行信息。目前正在构建用户操作监听机制。

我还准备了一些图片和代码来展示进度。

---

接下来是后续规划。 我计划是用接下来一周左右的事件实现前后端通信机制，然后再用一周多的时间来完成服务器和用户界面的部分。与此同时，继续完善插件的服务器接口封装，以及设计如何简单地修改界面。最后还有一周左右的时间可以进行bug修复，以及生产环境打包和集成测试。在整个3-4周的时间，可能还会对核心API进行查漏补缺的工作，修复bug以及添加或修改少量功能。

---
以上是我的中期汇报。谢谢大家的聆听。
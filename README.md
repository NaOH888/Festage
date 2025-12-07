![rain!](rain.webp)
🎮 Festage 项目说明文档（README.md）
📌 项目简介

Festage 是一个基于 Unity 引擎开发的项目，本仓库包含完整的工程源码。请按照以下步骤正确克隆并配置运行环境，否则可能会出现依赖报错问题。

✅ 环境要求

Unity Hub（建议 2021 LTS 或以上）

Git

Windows 系统（已测试）

🚀 一、项目克隆步骤

在你希望存放项目的文件夹中：

右键 → 选择 Open Git Bash Here

输入以下命令并回车：

git clone https://github.com/NaOH888/Festage/


等待克隆完成后，你会得到一个名为 Festage 的项目文件夹。

🛠 二、使用 Unity 打开项目

打开 Unity Hub

点击右上角 Open

选择刚刚克隆的 Festage 文件夹

点击 打开项目

首次打开时可能会出现 3 类报错，属于正常现象，请按下面方法依次解决。

❗ 三、常见报错及解决方法（重要）
✅ ① MediaPipe 相关报错解决方法

错误原因：
项目缺少 com.github.homuler.mediapipe 依赖包。

解决步骤：

打开 群里提供的 com.github.homuler.mediapipe 文件夹

将整个文件夹 复制

粘贴到以下路径中：

Festage/Packages


返回 Unity，等待自动重新编译即可

✅ ② TextMeshPro（TMP）报错解决方法

错误原因：
项目未安装 TextMeshPro 官方 UI 组件。

解决步骤：

在 Unity 顶部菜单栏点击：

Window → Package Manager


左上角切换为：

Unity Registry


搜索：

TextMeshPro


点击 Install / Download 安装

✅ ③ Unity Visual Scripting 报错解决方法

错误原因：
项目使用了可视化脚本插件但未安装。

解决步骤：

打开：

Window → Package Manager


仍然选择：

Unity Registry


搜索：

Visual Scripting


点击 Install / Download

✅ 四、全部依赖安装完成后

完成以上三步后：

Unity 会自动重新编译

所有报错应消失

项目即可正常运行 ✅
![rain!!](rain-s-feet.webp)
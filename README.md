# UWPTools

封装了一些个人常用的UWP C#帮助类与控件

## 下面介绍该项目中封装的部分控件与帮助类：

### 控件：

- DevTools
  执行开发阶段常用的操作，如：打开ApplicationData.Current.LocalFolder等快捷操作。

- ExplorerControl
  
  可以内嵌在软件内部的文件资源管理器控件，提供一些常用的文件操作功能。

### 帮助类：

- SQLiteConnection
  
  便捷的SQLite操作类，提供执行无返回值语句、单值查询、查询单个实体、查询实体列表、分页查询分页实体列表的快捷方法。

- SystemHelper
  
  提供查询设备种类的方法与枚举。

- StorageHelper
  
  提供创建文件（文件夹），检查文件（文件夹）是否存在、以二进制读取文件等方法。

- JSONHelper
  
  提供格式化JSON数据方法。

- SettingsManager
  
  提供利用ApplicationDataContainer实现应用设置的方法集。



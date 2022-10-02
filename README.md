# TippingExcelTool

[![license](http://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Unity Version](https://img.shields.io/badge/unity-2021.3.8f1-blue)](https://unity.com)
[![Platform](https://img.shields.io/badge/platform-Win%20%7C%20Android%20%7C%20iOS%20%7C%20Mac%20%7C%20Linux-orange)]()

# 简介
Unity读取Excel工具，自动生成C#索引字段，能读取二进制文件和Excel文件，支持Win/Android/iOS/Mac/Linux

## 使用流程
导入文件,查看DemoLauncher.cs，运行SampleScene场景

## excel格式速览
基础类型
|int| long | float | double | str |
| - | - | - | - | - |
| id | long1 | float1 | double1 | str1 |
|1|9935434343|2.725412|1.346655321|读取Excel工具|
|2|9935434343|2.725412|1.346655321|读取Excel工具|

拓展类型
| l_int | l_float | l_str | l_obj | l_l_int | l_l_float | l_l_str |
| - | - | - | - | - | - | - |
| l_int1 | l_float1 | l_str1 | l_obj1 | l_l_int1 | l_l_float1 | l_l_str1 |
|[1,5]|[1.5,5.8]|[文件,支持]|["生成",656,1.235999]|{[1,6],[2,8]}|{[6.215,6.12],[2.5,14.556]}|{[自动,格式],[tipping,excel]}|
|[1,5]|[1.5,5.8]|[文件,支持]|["生成",656,1.235999]|{[1,6],[2,8]}|{[6.215,6.12],[2.5,14.556]}|{[自动,格式],[tipping,excel]}|

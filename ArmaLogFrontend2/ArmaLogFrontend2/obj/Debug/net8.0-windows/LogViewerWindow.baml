   M S B A M L   `   `   `  ����  ArmaLogFrontend2%    ArmAGamingWorld.LogViewerWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Live Backend Log��5   	   $	��600��5   	   $	��800��5   	    � WindowStartupLocation$ CenterOwner=�5   	   .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��0��6.   $��5q�6;   .+�5      �� -   5         ��Name$ StopLogsButton��5      $��	Stop Logs��5      $��0,0,10,0q�5      5      �� 5      -   5         ��Name$ StatusTextBlock��5      $��Status: Running��5      $��Center=�5      5      �� 5   
   -   5         ��Name$ 
LogTextBox��5      $��1��5      $��5q�5       �� 
IsReadOnly$
 True��5       �� TextWrapping$ NoWrap=�5      "	 �� VerticalScrollBarVisibility$
	 Auto=�5      $
 �� HorizontalScrollBarVisibility$

 Auto=�5       5!      5"      
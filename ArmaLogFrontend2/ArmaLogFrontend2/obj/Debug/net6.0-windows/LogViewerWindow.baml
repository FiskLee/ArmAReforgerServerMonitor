   M S B A M L   `   `   `  ����  ArmaLogFrontend2%    ArmaLogFrontend.LogViewerWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  
Log Viewer��5   	   $	��450��6   $	��800��6)   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $��* �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��0��6.   $��10q�6;   .+�5      �� -   5         ��Name$ StopLogsButton��5      $��	Stop Logs��6-   $��80��5   2   $��0,0,10,0q�6=   �� 5      -   5         ��Name$ StatusTextBlock��5      $��Status: ��61   $��Center=�5      5      �� 5   
   $��1��6   $
��Auto=�6$   $
��Auto=�5      $��10q�6<   .��5      �� -   5         ��Name$ 
LogTextBox��5       �� 
IsReadOnly$
 True��6*    �� TextWrapping$ NoWrap=�6<   5      5      5      
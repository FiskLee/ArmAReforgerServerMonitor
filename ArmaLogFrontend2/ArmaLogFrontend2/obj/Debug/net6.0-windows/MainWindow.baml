   M S B A M L   `   `   `  ����U  QLiveCharts.Wpf, Version=0.9.7.0, Culture=neutral, PublicKeyToken=0bc1f845d1ebb8dfG4clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.WpfLiveCharts.Wpf  5   	    ArmaLogFrontend2    ArmaLogFrontend.MainWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Dlvc4clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf     5   	     � Title$F  @Arma Reforger Server Monitor Frontend (Made By ArmAGaming.World)��5   	   $	��800��5   	   $
��1200��6   .��5      �  � RowDefinitions 5   
   �� 5	      $
��Auto �6   �� 5
      $��* �6   �� 5      $
��Auto �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��10q�6.   $��0��6:   .+�5      �� $��	Username:��6   $��Center=�6)   $��0,0,5,0q�6D   �� 5      -   5         ��Name$ UsernameTextBox��5      $	��100��6/   $��0,0,15,0q�6;   �� 5      $��	Password:��6   $��Center=�6)   $��0,0,5,0q�6D   G� 5      -   5         G�Name$ PasswordBox��5      $	��100��6/   $��0,0,15,0q�6;   �� 5      $��Server URL:��6   $��Center=�6+   $��0,0,5,0q�6F   �� 5      -   5        $ ServerUrlTextBox��5      $	��150��60   $��http://localhost:5000��6<   $��0,0,15,0q�6Y   �� 5      -   5         ��Name$ ConnectButton��5      $��Connect��6,   $��70��6Z   $��0,0,5,0q�6e   �� 5      -   5        $ DisconnectButton��5      $��
Disconnect��6/   $��80��6c   $��0,0,15,0q�6n   �� 5      -   5         ��Name$ ConnectionStatusText��5      $��Status: Disconnected��66   $��Center=�6R   $��0,0,15,0q�6m   
���  ��6   �� 5      -   5        $ ActivePlayersLabel��5      $��Active Players: 0��64   $��Center=�6M   $��0,0,15,0q�6h   
��� � �6z   5      �� 5!   
   -   5         ��Name$ MainTabControl��5!      $��1��6-   $��10q�6:   .@�5#      �� $��Performance��6   .��5$      �� .��5%      �� $��10q�6!   .+�5'      �� $ 
Horizontal=�6%   $��0,0,0,10q�6>   .+�5(      �� $��Time Range:��6(   $��Center=�6;   $��0,0,5,0q�6V   �� 5)      -	   5         ��Name$ TimeRangeComboBox��5)   '   $	��120��6B   .@�5*   "   �� $��Last 10 min��6/   	 �� 
IsSelected$
	 True��6E   �� 5+   "   $��Last 30 min��6/   �� 5,   "   $��Last 1 hour��6/   �� 5-   "   $��Last 24 hours��6/   5.      �� 5/      -
   5        $ PausePerformanceButton��5/   %   $��Pause Performance��6E   $��15,0,0,0q�6�   �� 50      -   5        
 ��Name$
 UseGbCheckBox��50   '   $��Use GB��6>   $��15,0,0,0q�6O    d� 	IsChecked$
 TrueY�6a   6r   51      �� 54      -   5        $ PerformanceSummaryText��54   $   $_��YCPU: 0% | Active Memory: 0.00 GB | DiskR:0MB/s | DiskW:0MB/s | NetIn:0MB/s | NetOut:0MB/s��55   $   $��0,0,0,10q�56   $   $
��Bold"�57   $   �� 5:      $��Raw Performance Data:��6$   $
��Bold"�6A   �� 5;      -   5        $ RawPerformanceDataTextBox��5;   "   $	��100��6E   " �� VerticalScrollBarVisibility$
 Auto=�6R   $ �� HorizontalScrollBarVisibility$
 Auto=�5<   "   $��(No data yet)��6G   $��0,0,0,10q�6\   �� 5?      $��FPS (Bar Chart, 0-60)��6#   $��0,0,0,10q�6B   .��5@      #   LiveCharts.Wpf.CartesianChart  -   5          Name$ FpsChart��5@   1   $	��200��6C   &    LiveCharts.Wpf.Charts.Base.Chart   AxisY 5B   &      LiveCharts.Wpf.Axis     Title$	 FPS��6/      MinValue$ 0Q�6;      MaxValue$ 60Q�6H      AxisX 5E   &     $
 Time��6/   5G      5H      �� 5K      $!��Frame Time (ms) - Bar Chart��6#   $��0,0,0,10q�6H   .��5L        -   5        $ FrameTimeChart��5L   1   $	��200��6I    5N   &     $ ms��6/   $ 0Q�6:    5Q   &     $
 Time��6/   5S      5T      �� 5W      $%��Player Count (Bar Chart, 0-128)��6#   $��0,0,0,10q�6L   .��5X        -   5        $ PlayerCountChart��5X   1   $	��200��6K    5Z   &     $ Players��6/   $ 0Q�6?   $	 128Q�6L    5]   &     $
 Time��6/   5_      5`      �� 5c      $#��CPU Usage (Bar Chart, 0-100%)��6#   $��0,0,0,10q�6J   .��5d        -   5        $ CpuUsageChart��5d   1   $	��200��6H    5f   &     $ CPU %��6/   $ 0Q�6=   $	 100Q�6J    5i   &     $
 Time��6/   5k      5l      �� 5o      $,��&Individual CPU Cores usage (Bar Chart)��6#   $��0,0,0,10q�6S   .��5p        -   5        $ CpuCoresChart��5p   1   $	��200��6H    5r   &     $ Core %��6/   $ 0Q�6>   $	 100Q�6K    5u   &     $ Core / Time��6/   5w      5x      5y      5z      5{      �� 5~      $��Players��6   .��5      �� $��10q�6   .+�5�      �� $ 
Horizontal=�6!   $��0,0,0,10q�6:   .+�5�      �� -   5        $ ShowPlayersButton��5�   !   $��Show Players in Database��6<   $	��180��6   $��0,0,10,0q�6�   �� 5�      -   5        $ RefreshPlayersButton��5�   !   $��Refresh��6?   $��80��6t   5�      �� 5�      -   5         ��Name$ PlayersListBox��5�      $	��400��66   5�      5�      �� 5�      $��Raw Data��6   .��5�      �� $��10q�6   .+�5�      �� $ 
Horizontal=�6!   $��0,0,0,10q�6:   .+�5�      �� -   5        $ ShowRawDataButton��5�   !   $��Show Raw Data��6<   $	��120��6t   $��0,0,10,0q�6�   �� 5�      -   5        $ PauseRawDataButton��5�   !   $��Pause Raw Data��6=   $	��120��6w   5�      �� 5�      -   5        $ RawDataTextBox��5�      $	��400��66   $
 Auto=�5�      $
 Auto=�5�      $��(Raw data disabled)��5�       �� 
IsReadOnly$
 True��5�      5�      5�      �� 5�      $
��Logs��6   .��5�      �� $��10q�6   .+�5�      �� $ 
Horizontal=�6!   $��0,0,0,10q�6:   .+�5�      �� -   5        $
 ScrollLockCheckBox��5�   #   $��Lock Scroll��6?   $
 TrueY�6U   $��0,0,10,0q�6f   �� 5�      -   5        $ FetchBackendLogsButton��5�   !   $��Fetch Backend Logs��6A   $	��140��6�   $��0,0,10,0q�6�   �� 5�      -   5        $ ShowFrontendLogsButton��5�   !   $��Show Frontend Logs��6A   $	��140��6�   5�      �� 5�      -   5        $ NoLogsLabel��5�       $��(No logs yet)��65   
�������$��0,0,0,10q�6\   �� 5�      -   5         ��Name$ LogsScrollViewer��5�   #   $
��Auto=�6=   $
��Auto=�6`   $	��400��6�   .��5�      �� -   5        $ LogsTextBox��5�   "    �� AcceptsReturn$
 True��67   $
 True��6L   6^   5�      5�      5�      5�      �� 5�   
   -   5        $ StatsUpdatedLabel��5�      $��Stats updated: N/A��6/   $��2��6I   $��	10,0,0,10q�6V   
�������$��Bottom=�6{   5�      5�      
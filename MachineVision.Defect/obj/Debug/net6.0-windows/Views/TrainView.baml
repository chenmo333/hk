   M S B A M L   `   `   `  ����O  KMachineVision.Defect, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:MachineVision.Defect.ViewsMachineVision.Defect.Views  5      *    $MachineVision.Defect.Views.TrainView   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      :local(clr-namespace:MachineVision.Defect.Views     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      $	��800��5      $	��450��5	      .��5      �   � RowDefinitions  5   
   �� 5      $
��auto �6   �� 5      $
��auto �6   �� 5      �� 5      $
��auto �6   .+�5   
   �� $��10q�5      $��22(�5      $
��Bold"�5      $��模型管理��5      �� 5   
   $��1��5      $��10q�5       �� Orientation$ 
Horizontal=�5      .+�5      �� $��Center=�6   $��检测区域:��63   �� 5      $	��120��5      $
��10,0q�5       ��5!      �� *
RegionList+ �� SelectedItem 5"      �� *SelectedRegion+��5#      �� 5$      .R�5%      �� ��6$   �� *Name+5&      5(      5)      �� 5+   
   $��1��5,      $
��10,0q�5-      $��Right=�5.      
���  ��$��0q�50      $��删除��52      ��51      �� *DeleteCommand+52      � 54   
   $��2��6    � ColumnDefinitions 55      �� 56      $
��auto �6#   �� 57      .+�5:      �� ��6   �� *Files+ 64   �� *SelectedFile+��5;      �� 5<      .R�5=      �� ��6$   �� *
FileName+5>      5@      �� 5B      $��1��6   ��6$   �� *Image+5C      �� 5E   
   $��3��5F      $��15q�5G      $��Right=�5H      $ 
Horizontal=�5I      .+�5J      �� $
��20,5q�5K      $��确定��5M      ��5L      �� *SaveCommand+5M      �� 5N      $��15,0,0,0q�5O      $
��20,5q�5P      
�������$��0q�5R      $��取消��5T      
���   ���5S      �� *CancelCommand+5U      5V      5W      5X      
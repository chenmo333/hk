   M S B A M L   `   `   `  ����  MachineVision"    MachineVision.Views.MainView   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      _ [Microsoft.Xaml.Behaviors, Version=1.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a3i+http://schemas.microsoft.com/xaml/behaviors  5      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5      _ [MaterialDesignThemes.Wpf, Version=4.9.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a:md1http://materialdesigninxaml.net/winfx/xaml/themes  5      [	 WPrism.DryIoc.Wpf, Version=8.1.97.5141, Culture=neutral, PublicKeyToken=40ee6c3a2184dc59T
 PPrism.Wpf, Version=8.1.97.5141, Culture=neutral, PublicKeyToken=40ee6c3a2184dc59&prismhttp://prismlibrary.com/ 	 
 5	        � Title$  机器视觉检测平台��5
      $
��1400��5      $	��960��5      ! 
 Prism.Mvvm.ViewModelLocator   AutoWireViewModel True5         MaterialDesignPaper#���   5        MaterialDesignBody#���  5       � WindowStartupLocation$ CenterScreen=�5       � WindowState$ 	Maximized=�5      .��5      )  #MaterialDesignThemes.Wpf.DialogHost     
Identifier$
 Root��6   .��5   
   �  � RowDefinitions 5      �� 5      $
��auto �6    �� 5      .+�5       (  "MaterialDesignThemes.Wpf.ColorZone  .  (MaterialDesignThemes.Wpf.ElevationAssist   	Elevation$	 Dp2=�6      Mode$ PrimaryDark=�6>   .��5!      �  � ColumnDefinitions 5"      �� 5#      $
��auto �6+   �� 5%      .+�5*      �� $��10,15,15,15q�5+      $��22(�5,      $
��Bold"�5-      
�������  ApplicationName#���  5/      *  $Microsoft.Xaml.Behaviors.Interaction	   Triggers	 50      +  %Microsoft.Xaml.Behaviors.EventTrigger  51      
   	EventName$
 MouseLeftButtonDown��6-   *  $Microsoft.Xaml.Behaviors.TriggerBase   Actions. 53   "   2  ,Microsoft.Xaml.Behaviors.InvokeCommandAction     Command 68   �� *GoHomeCommand+54      56      �� 59      -   5         ��Name$ listMenuBox��5:      $��1��5;      ��5<      �� *NavigationService.Items+��5>      �� 5?      .R�5@   "   ��  �� Orientation$ 
Horizontal=�6-   5A      ��5E      �� 5F       �� 
TargetType" ��6$   .�5G   "   ��  �� Property �@�FocusVisualStyle �� Value 6E   Z� �� 5H   "    � �� 5I   &   �� 5J   *    �� 
TargetType" ��6:   .R�5K   .   � .+�5L   2   ��  ��Name$ border��69   �� 5M   2   $ borderHeader��69   #��z��6O   �� 5N   2   #��z��5O   5   #��z��5P   5   5Q   /    �� Triggers 5T   .   P� 5U   2    P� Property �@��
IsSelected P� Value$
 True��6P   . �5V   6   ��  �� 
TargetName$ borderHeader��6=    � �� �#ffffff5W   =   �� 5X   6   $ borderHeader��6=    �@J�Opacity$	 0.1Q�6j   �� 5Y   6   $ border��6=    � �� �#ffffff6h   �� 5Z   6   $ border��6=    � ��$ 0,0,0,3q�5[   =   5\   3   P� 5^   2    �@J�IsMouseOver$
 True��6Q   . �5_   6   �� $ borderHeader��6=    � �� �#ffffff5`   =   �� 5a   6   $ borderHeader��6=    �@J�Opacity$	 0.1Q�6j   �� 5b   6   $ border��6=    � �� �#ffffff6h   �� 5c   6   $ border��6=    � ��$ 0,0,0,3q�5d   =   5e   3   5g   +   5i   #   5j      ��5n      �� 5o      .R�5p   "   �� $
��20,5q�5q   %   $��16(�5r   %   
���������5t   %   �� *Name+5u      	 5x        5y      $
 SelectionChanged��6-   . 5{   "      68   �� *NavigateCommand+   CommandParameter 5|   8   ��  �� ElementName$ listMenuBox�� �� Path$ SelectedItem�5}      5      5�      5�        5�      $��1��6   .��5�      )	  #MaterialDesignThemes.Wpf.DrawerHost	  ��6    �� $ MainViewContentControl��$ Content� �� Mode$ TwoWay=� 	  IsTopDrawerOpen 5�       �� *IsTopDrawerOpen+ 	  TopDrawerContent 5�      �� 5�      ��6'   �� *" NavigationService.Items[0].Items+��5�      �� 5�   "   .R�5�   &   � 5�   #   ��5�      �� 5�   "   .R�5�   &   �  5�   *   �� 5�   .   $
��auto �6<   �� 5�   .   .+�5�   *   �� $��15,10q�5�   -   $
��Bold"�5�   -   ��5�   -   �� *Name+�� 5�   *   $��1��5�   -   $��10q�5�   -   ��5�   -   �� *Items+��5�   .   �� 5�   2   .R�5�   6   A�  A� Columns$ 2��6B   5�   3   ��5�   .   �� 5�   2   .R�5�   6   �� $��5q�5�   9   ��5�   9   �� *DataContext.NavigateCommand+ �� RelativeSource ��   �� Mode$  FindAncestor=�! �� AncestorType
! Window��5�   9   �� ��5�   9   �� *Name+5�   3   5�   +   5�   '   5�   #   5�      5�      5�      �� 5�      -   5        " ��Name$" MainViewContentControl��5�      !
 
 Prism.Regions.RegionManager# 
  
RegionName$# MainViewRegion��5�      $y�	Collapsed=�5�      5�      5�      5�      
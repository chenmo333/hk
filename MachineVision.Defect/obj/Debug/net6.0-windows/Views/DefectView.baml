   M S B A M L   `   `   `  ����O  KMachineVision.Defect, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:MachineVision.Defect.ViewsMachineVision.Defect.Views  5      +    %MachineVision.Defect.Views.DefectView   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      :local(clr-namespace:MachineVision.Defect.Views     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      _ [MaterialDesignThemes.Wpf, Version=4.9.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a:md1http://materialdesigninxaml.net/winfx/xaml/themes  5      .��5      �   � RowDefinitions  5   
   �� 5      $
��auto �6   �� 5      $
��auto �6   �� 5      .+�5   
   �� $��15q�5      $��22(�5      $
��Bold"�5      $ ��缺陷检测(项目管理)��5      �� 5   
   $��1��5      $��15q�5       �� Orientation$ 
Horizontal=�5      .+�5      �� $��Center=�6   $��搜索条件:��63   �� 5      $	��200��5      $
��10,0q�5       $��5q�5!      $��Center=�5"       !  MaterialDesignOutlinedTextBox#��[  5#      ��5$      �� *
FilterText+�� 5%      $��搜索��67   ��6   �� *SearchCommand+67   5&      �� 5(   
   $��1��5)      $
��15,0q�5*      $��Right=�5+      $��新建项目��5-      ��5,      �� *CreateCommand+5-      &   System.Windows.Controls.DataGrid  5/   
   $��2��50         AutoGenerateColumns$ False��51         CanUserAddRows$ False��52         
IsReadOnly$
 True��53      ��54      �� *Models+   Columns 55      0  *System.Windows.Controls.DataGridTextColumn  56      ,  &System.Windows.Controls.DataGridColumn   Header$ 序号��6<   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *Id+6<     57      $ 项目名称��6>    6%   �� *Name+6>     58      $ 创建日期��6h    6%   �� *
CreateDate+ �� StringFormat$ yyyy-MM-dd hh:mm:ss��6h     59      $ 更新日期��6h    6%   �� *
UpdateDate+$ yyyy-MM-dd hh:mm:ss��6h   4  .System.Windows.Controls.DataGridTemplateColumn  5:      $ 操作��6)   	   CellTemplate	 5;      �� 5<      .R�5=      �� $ 
Horizontal=�6)   .+�5>   "   �� 
��� � �$��0q�5@   %   $��选择��5C   %   ��5A   %   �� *DataContext.EditCommand+
 �� RelativeSource
 ��  �� Mode$ FindAncestor=� �� AncestorType DataGrid��5B   %   �� 5C   %   �� 5D   "   $	��5,0q�5E   %   
���  ��$��0q�5G   %   $��删除��5J   %   ��5H   %   �� *DataContext.DeleteCommand+
 �� $ FindAncestor=� DataGrid��5I   %   �� 5J   %   5K      5L      5N      5P      5Q      5R      
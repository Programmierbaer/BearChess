   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5      G    Awww.SoLaNoSoft.com.BearChessWpfCustomControlLib.QueryDialogWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   
   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5      -   5          � SizeToContent$  WidthAndHeight=�5       � WindowStyle$
 None=�65   $��20(�6_    � WindowStartupLocation$ CenterOwner=�5   	    � Title$ QueryDialogWindow��5	      $	��450��6   $	��800��6,    � 	Resources 5
         CtrQ �� Text  CtrF  CtrJ  CtrY  CtrN  CtrZ  CtrB�� 5   
   %  &	        &	      &	 >     &	 [     &	 x     &	 �     &	 �     �� 5      $ Query��6+   �� 5      $ Query��6+   �� 5      $	 Yes��6+   �� 5      $	 Yes��6+   �� 5      $ No��6+   �� 5      $
 Prev��6+   �� 5      $
 Prev��6+   5       J� InputBindings 5      �� 5   
   
 �� Key$ Q��6    �� 	Modifiers$
 Ctrlf�6   	 �� Command#	 [  6.   �� 5   
   $ F��6   $
 Ctrlf�6   #	 [ 6.   �� 5   
   $ Y��6   $
 Ctrlf�6   #	 [ 6.   �� 5   
   $ J��6   $
 Ctrlf�6   #	 [ 6.   �� 5   
   $ N��6   $
 Ctrlf�6   #	 [ 6.   �� 5   
   $ Z��6   $
 Ctrlf�6   #	 [ 6.   �� 5   
   $ P��6   $
 Ctrlf�6   #	 [ 6.   
 J� CommandBindings
 5      )  #System.Windows.Input.CommandBinding  5!   
   -   5           Command# [  5!      69     5"   
   -   5        # [ 5"      69     5#   
   -   5        # [ 5#      69     5$   
   -   5        # [ 5$      69     5%   
   -   5        # [ 5%      69     5&   
   -   5        # [ 5&      69     5'   
   -   5        # [ 5'      69   .��5+      ��  +� FocusedElement 6   ��  �� ElementName$ 	YesButton��.+�5,   
   �� -	   5        $��TextBlockQuestion��5,       $��10q�6   6;   �� 5-   
   -
   5        $��	YesButton��5-   6   $��10q�6   $��1��6G   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources
   Yes4  .System.Windows.Automation.AutomationProperties   HelpText# Z 6T   #��Z 5.   +   6R   �� 5/   
   -   5        $��NoButton��5/   5   $��10q�6   $��2��6E   	   No#��Z 6p   # Z 6�   6�   �� 50   
   -   5        $��
PrevButton��50   7   $��10q�6   $��3��6I      PrevQuestion# Z 6u   #��Z 6�   6�   �� 51   
   -   5        $��CancelButton��51   9   $��10q�6   $��4��6M      QueryDialogCancel# Z 6y   #��Z 6�   6�   52      53      
   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows'www.SoLaNoSoft.com.BearChessWin.Windows  5   	   g8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   2    ,www.SoLaNoSoft.com.BearChessWin.PlayerWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:www.SoLaNoSoft.com.BearChessWin.Windows     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   -   5        $��16(�5   P     � SizeToContent$  Height=�5   M    � WindowStartupLocation$ CenterOwner=�5       � 
ResizeMode$ NoResize=�6?    � Icon$) #../Assets/Icons/user_silhouette.png��6U   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   Player � Title# Z 5	   	   $	��140��6/   $	��220��6<    � 	Resources 5
         CtrEnter �� Text�� 5      %(   &	        �� 5   
   $
 Exit��6+   5       J� InputBindings 5      �� 5   
   
	 �� Key$	 Enter��6   
 �� 	Modifiers$

 Ctrlf�6!    �� Command# [  62    J� CommandBindings 5      )  #System.Windows.Input.CommandBinding  5   
   -   5           Command# [  5      6=   .��5      �  � ColumnDefinitions 5   
   �� 5      $
��Auto �6   6.   �� 5      $��* �6   6+    � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   �� $��5q�6   $��0��6   $��0��6/   $��SemiBold"�6<      	FirstName#��Z 6R   �� 5    
   -   5        $��textBlockFirstName��5       $��5q�6   $��1��67   $��0��6G   4  .System.Windows.Automation.AutomationProperties   HelpText# Z 6}   �� 5!   
   $��5q�6   $��0��6   $��1��6/   $��SemiBold"�6<      LastName#��Z 6R   �� 5"   
   -   5        $��textBlockLastName��5"      $��5q�6   $��1��66   $��1��6F   # Z 6|   ]� 5#   
   $��0��6   $��2��6$   $��2��68   $��5q�6E   $��Bottom=�6P   ��.  .+�5$      �� -   5        $��buttonOk��5$      $��5,5,5,5q�6D   $
��Left=�6U   $��28��6l   $��28��6w   
������ 
������  �� 	IsDefault . 	   Ok � ToolTip# Z 5%      .��5&      �� $'��!../Assets/Icons/accept_button.png��6   6F   5'      �� 5(      -   5        $��buttonCancel��5(      $��5,5,5,5q�6M   $��Right=�6^   $��28��6v   $��28��6�   
������ 
������  �� IsCancel .    Cancel# Z 6�   .��5)      �� $ ��../Assets/Icons/cancel.png��6   6@   5*      5+      5,      5-      
Module Module1

    Sub Main()
        'Example of the application of those functions
        Dim i As Integer
        Dim j As Integer
        Dim T As Double = 20.0
        'Eclosion means : LarvaState(0) = 0 (if LarvaState(0)=-1)
        Dim LarvaState() As Double = {0,-1,-1,-1}
        j=1
        '0.5 is for safety it prevents an infinite loop (comparing to an higher percentage)
        Do While LarvaState(3)<0.5
            LarvaState = ChangeState(LarvaState, T)
            Console.WriteLine("Larvar State Day " & j & " :")
            For i = 0 To 3 'LarvaState.Length
                Console.WriteLine("L" & i + 1 & " : " & LarvaState(i) )
            Next
            j=j+1
        Loop
    End Sub

    'Function updating the lavaire state
    'By Morgan Bruhin
    'PS didn't use "degre-jour" didn't understand well enought it
    'Using percentage of grow per state

    'Advantage : would allow to have more than one state per polygone (more than only one larval type)

    'Function that compute the Increase percentage for state cs at temperature T for 1 day
    Function LarvaModel(ByVal cs As Integer, ByVal T As Double) As Double
        
        Dim Prop As Double = 0.0
        Dim i As Integer
        Dim j As Integer
        Dim noCol As Integer = 5
        Dim Model(4,noCol) As Double
        
        'Reference at the bottom
        Dim temp() As Double = {15,20,25,27,30,34}
        Dim L1() As Double = {7.67, 2.67, 2.74, 0.96, 1.18, 1.42}
        Dim L2() As Double = {8.88, 1.43, 1.35, 0.77, 0.89, 1.31}
        Dim L3() As Double = {14.97, 1.62, 1.37, 0.96, 0.98, 0.84}
        Dim L4() As Double = {15.31, 3.59, 3.15, 1,78, 1.94, 1.49}
        
        'Could probably be inproved
        For i = 0 To noCol
            Model(0,i) = temp(i)
            Model(1,i) = L1(i)
            Model(2,i) = L2(i)
            Model(3,i) = L3(i)
            Model(4,i) = L4(i)
        Next
        If T < Model(0,0) Or T > Model(0, noCol) Then
            Console.WriteLine("Temperature " & T &"°C out of range [ "& Model(0,0) &":" & Model(0, noCol) & " ]")
        Else
            For i = 0 To noCol-1
                If T>=temp(i) And T<temp(i+1) Then
                    'Compute a linear estimation of the require number of days to grow up
                    'then compute the actual proportion of growing since the simulation is at a daily scale
                    Prop = 1 / ( Model(cs,i) + ( T - temp(cs,i) ) / ( temp(cs,i+1) - temp(cs,i) ) * ( Model(cs,i+1) - Model(cs,i) ) )
                End If
            Next
        End If
        'Display test
        'For i = 0 To 4
         '   For j= 0 To noCol
          '      Console.WriteLine("Model "& Model(i,j))
           ' Next
        'Next
        
        Return Prop
    End Function

    'Should be a method but I'mnot good at it
    'Function that modify the Percentage in the LarvaStates and return the new one after applying the LarvalModel to each require state
    'By Morgan Bruhin
    Function ChangeState( ByVal PercLarvaState() As Double, ByVal T as Double) As Double()
        Dim i As Integer
        Dim index As Integer = -1
        For i = 0 To 3
            'if the larva aren't present at the current state i then the percentage is -1
            If PercLarvaState(i) <> -1 Then
                'Increase the Larva growth percentage
                PercLarvaState(i) = PercLarvaState(i) + LarvaModel(i,T) 
                'if the percentage is up to 1 the state is complete
                If PercLarvaState(i)>=1.0 Then
                    'if the state isn't the last one activate the next
                    If i<3 Then
                        If PercLarvaState(i+1)=-1 Then
                            index=(i+1)
                        Else
                            'TODO : Decide what to do if a younger population arrive befor the previous fully grows
                        End If
                    Else
                        'TODO : decide what to do exactly for T4 fully grow
                    End If
                    'At the end : desactivate the current state
                    PercLarvaState(i)=-1
                End If
                'Activate the next state without computing a new day
            Else If i=index Then
                PercLarvaState(i)=0 ' TODO : Compute the difference with the previous state and compute a proportional increase
            End If
        Next
        
        Return PercLarvaState
    End Function

    
End Module

'Require duration in days to grow (at T)
'( L.M. Rueda et al. 1990. Temperature-Dependent Development and survival Rates of [...] Aedes aegypti )
'Dim L1() As Double = {84,60,52.5,42,59.5}
'Dim L2() As Double= {49.5,28,30,39,42}
'Dim L3() As Double = {69,42,30,42,84}
'Dim L4() As Double = {201,82,82.5,90,238}

Option Strict On
Option Explicit On

Public Class Mousticator
    'classe de séparation des débits

    Inherits HydroObject

    'Private mz As Single = 677.0F
    Private mLarvalState_Ini As Single = 1.0F, mLarvalState As Single = 1.0F
    Private mDaysFromLastEclosionIni As Integer = 0, mDaysFromLastEclosion As Integer = 0


    'VARIABLES PARAMETERS INPUTS/OUTPUTS/RESULTS
    Private __z As String = "z"
    Private __TUp As String = "TUp"
    Private __Level As String = "Level"
    Private __LarvalState As String = "LarvalState"
    Private __DaysFromLastEclosion As String = "DaysFromLastEclosion"

    'PARAMETERS
    Private __z_parameter As Integer = 0
    Private __LarvalState_parameter As Integer = 1
    Private __DaysFromLastEclosion_parameter As Integer = 2

    'INPUT/OUTPUT/RESULT
    Private __TUp_input As Integer = 0
    Private __Level_input As Integer = 1
    Private __LarvalState_result As Integer = 0
    Private __DaysFromLastEclosion_result As Integer = 1

    Public Sub New()

        'Fabrication de mes paramètres
        Dim P As Parameter
        MyBase.Type = ObjectsType.Mousticator

        'Mes paramètres
        '0
        P = New Parameter(ParameterTypeEnum.H, __z, "(m a.s.l.)")
        P.ParamValue = 677.0F
        MyBase.Parameters.Add(P)
        P = Nothing
        '1
        P = New Parameter(ParameterTypeEnum.NotDefined, __LarvalState, "(-)")
        P.ParamValue = 1.0F
        MyBase.Parameters.Add(P)
        P = Nothing
        '2
        P = New Parameter(ParameterTypeEnum.NotDefined, __DaysFromLastEclosion, "(-)")
        P.ParamValue = 0.0F
        MyBase.Parameters.Add(P)
        P = Nothing



        'mes Inputs
        '0
        P = New Parameter(ParameterTypeEnum.Temperature, __TUp, "(°C)")
        P.Position = ParameterPosition.Upstream
        MyBase.Inputs.Add(P)
        P = Nothing
        '1
        P = New Parameter(ParameterTypeEnum.H, __Level, "(m a.s.l.)")
        P.Position = ParameterPosition.Upstream
        MyBase.Inputs.Add(P)
        P = Nothing

        'mes Outputs

        'mes résultats
        '0
        MyBase.Results.Add(New Result(New Parameter(ParameterTypeEnum.NotDefined, __LarvalState, "(-)")))
        '1
        MyBase.Results.Add(New Result(New Parameter(ParameterTypeEnum.NotDefined, __DaysFromLastEclosion, "(-)")))

        MyBase.InitIntegralOutputs()
    End Sub

    Public Property z As Single
        Get
            Return CSng(MyBase.Parameters(__z_parameter).ParamValue)
        End Get
        Set(ByVal value As Single)
            MyBase.Parameters(__z_parameter).ParamValue = value
        End Set
    End Property

    Public Property StadeLarvaire As Single
        Get
            Return CSng(MyBase.Parameters(__LarvalState_parameter).ParamValue)
        End Get
        Set(ByVal value As Single)
            MyBase.Parameters(__LarvalState_parameter).ParamValue = value
        End Set
    End Property

    Public Property DaysFromLastEclosion As Integer
        Get
            Return CInt(MyBase.Parameters(__DaysFromLastEclosion_parameter).ParamValue)
        End Get
        Set(ByVal value As Integer)
            MyBase.Parameters(__DaysFromLastEclosion_parameter).ParamValue = value
        End Set
    End Property

    Public Overrides Sub PrepareComputation()

        mLarvalState = mLarvalState_Ini
        mDaysFromLastEclosion = mDaysFromLastEclosionIni

    End Sub

    Public Overrides Sub ComputeOutput()


        'Evaluation du niveau et de la submersion
        Dim _Level As Single = CSng(MyBase.Inputs(__Level_input).Value)
        Dim _T As Single = CSng(MyBase.Inputs(__TUp_input).Value)

        If _Level >= Me.z Then


            If mLarvalState < 5.0F Then

                mLarvalState = Me.CalculateStadeLarvaire(mLarvalState, _T, mDaysFromLastEclosion)

            Else

                Dim TotalSecondsFromStart As Integer = CInt(Application.Solver.CurrentDate.Subtract(Application.Solver.DateStart).TotalSeconds)
                mDaysFromLastEclosion = mDaysFromLastEclosionIni + CInt(TotalSecondsFromStart / 86400.0F)

                If mDaysFromLastEclosion >= 15.0F Then
                    mDaysFromLastEclosion = 0
                    mLarvalState = 1.0F
                End If

            End If

        Else
            'A IMPLEMENTER

            'check how much time the zone has been without water. If it is less than 3 days, the development goes on. Otherwise, we consider the larvae dead.
        End If

        MyBase.IntegralOutputs(__LarvalState_result) += mLarvalState '* MyBase.IntegralCount
        MyBase.IntegralOutputs(__DaysFromLastEclosion_result) = mDaysFromLastEclosion

    End Sub

    Function CalculateStadeLarvaire(ByVal stadeLarvaire As Single, ByVal T As Single, ByVal daysFromLastEclosions As Integer) As Single

        'Declaration des variables de sortie
        Dim state_today As Integer = CInt(Math.Ceiling(stadeLarvaire) - 1)
        Dim percentage_today As Single = stadeLarvaire - state_today

        'catégories de temperature
        Dim temp() As Single = {15.0F, 20.0F, 25.0F, 30.0F, 35.0F}

        'Durée stades vexans (établi à l'aide du code matlab "adaptation_albopictus_vexans.m")
        Dim L1() As Single = {3.4F, 2.4F, 1.5F, 0.9F, 0.7F}
        Dim L2() As Single = {2.0F, 1.1F, 0.9F, 0.9F, 0.5F}
        Dim L3() As Single = {2.8F, 1.7F, 0.9F, 0.9F, 1.0F}
        Dim L4() As Single = {8.1F, 3.3F, 2.4F, 2.0F, 3.0F}

        'Durée stades albopictus
        'Dim L1() As Double = {5.6, 3.0, 2.1, 1.4, 1.7}
        'Dim L2() As Double = {3.3, 1.4, 1.2, 1.3, 1.2}
        'Dim L3() As Double = {4.6, 2.1, 1.2, 1.4, 2.4}
        'Dim L4() As Double = {13.4, 4.1, 3.3, 3.0, 6.8}


        Dim i As Integer
        Dim noCol As Integer = temp.Length - 1
        Dim Model(4, noCol) As Single
        Dim percentage_incr As Single 'MAX: cette variable montre l'augmentation du pourcentage de la maturation de l'état qui avait lieu entre hier et aujourd'hui
        Dim time_new_state As Single 'MAX: dans le cas où un changement d'état a lieu entre hier et aujourd'hui, cette variable indique le temps qui s'est écoulé depuis que ce nouvel état a lieu.

        For i = 0 To noCol
            Model(0, i) = temp(i)
            Model(1, i) = L1(i)
            Model(2, i) = L2(i)
            Model(3, i) = L3(i)
            Model(4, i) = L4(i)
        Next

        If T < Model(0, 0) Or T > Model(0, noCol) Then
            'Console.WriteLine("Temperature " & T & "°C out of range [ " & Model(0, 0) & ":" & Model(0, noCol) & " ]")


        Else

            For i = 0 To noCol - 1
                If T >= temp(i) And T < temp(i + 1) Then
                    'Compute a linear estimation of the require number of days to grow up
                    'then compute the actual proportion of growth since the simulation is at a daily scale
                    percentage_incr = CSng(1.0F / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i))))
                    If percentage_incr + percentage_today >= 1.0F Then
                        If stadeLarvaire = 4 Then
                            time_new_state = 0
                            state_today = 5
                            percentage_today = 0
                        Else
                            time_new_state = (percentage_incr + percentage_today - 1) * (1 / percentage_incr)
                            state_today += 1 ' this one gets out of range
                            percentage_today = time_new_state / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i)))
                            If percentage_today >= 1 Then
                                Dim break As Integer = 0
                                While break = 0
                                    state_today = state_today + 1
                                    time_new_state = (percentage_today - 1) * (1 / percentage_today)
                                    percentage_today = time_new_state / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i)))
                                    If percentage_today < 1 Then
                                        break = 1
                                    End If
                                End While
                            End If
                        End If
                    Else
                        time_new_state = 0

                        percentage_today += percentage_incr
                    End If
                End If
            Next
        End If

        Return state_today + percentage_today

    End Function

    Public Overrides Sub SaveStep()
        For i As Integer = 0 To MyBase.Results.Count - 1
            MyBase.Results(i).Add(MyBase.IntegralOutputs(i) / MyBase.IntegralCount)
            MyBase.IntegralOutputs(i) = 0
        Next
        MyBase.IntegralCount = 0
    End Sub

    Public Overrides Sub SaveFirstStep()

        MyBase.Results(__LarvalState_result).Add(mLarvalState_Ini)
        MyBase.Results(__DaysFromLastEclosion_result).Add(mDaysFromLastEclosionIni)

        MyBase.IntegralCount = 0
    End Sub

    Public Overrides ReadOnly Property GetInitialConditions() As System.Collections.Generic.List(Of Single)
        Get
            Dim L As New List(Of Single)
            L.Add(CSng(Me.Parameters(__LarvalState_parameter).ParamValue))
            L.Add(CSng(Me.Parameters(__DaysFromLastEclosion_parameter).ParamValue))
            Return L
        End Get
    End Property

    Public Overrides Sub DefaultStart(ByVal Coeff As Single)
        mLarvalState_Ini = CSng(Me.Parameters(__LarvalState_parameter).ParamValue)
        mDaysFromLastEclosionIni = CInt(Me.Parameters(__DaysFromLastEclosion_parameter).ParamValue)
    End Sub

    Public Overrides Sub SetStateParameters()
        Me.Parameters(__LarvalState_parameter).ParamValue = mLarvalState_Ini
        Me.Parameters(__DaysFromLastEclosion_parameter).ParamValue = mDaysFromLastEclosionIni
    End Sub

    Public Overrides Sub Hotstart(ByVal index As Integer, ByVal Coeff As Single, Optional ByVal UpdateRange As Single = 1000)
        'Débit initial constant dans le tronçon
        If Results(__LarvalState_result).X.Length >= index + 1 Then mLarvalState_Ini = Results(__LarvalState_result).X(index)
        If Results(__DaysFromLastEclosion_result).X.Length >= index + 1 Then mDaysFromLastEclosionIni = CInt(Results(__DaysFromLastEclosion_result).X(index))
    End Sub

    Public Overrides Sub Write(ByVal SW As System.IO.StreamWriter)
        SW.WriteLine("Mousticator")
        SW.WriteLine(1.001)
        MyBase.Write(SW)
    End Sub

    Public Overrides Sub Read(ByVal SR As System.IO.StreamReader)
        Dim V As Single
        V = CSng(SR.ReadLine)
        MyBase.Read(SR)
    End Sub
End Class

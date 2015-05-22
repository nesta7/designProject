Option Strict On
Option Explicit On

Public Class Mousticator

    Inherits HydroObject

    'Private mz As Single = 677.0F
    Private mStadeLarvaireIni As Single = 1.0F, mStadeLarvaire As Single = 1.0F
    Private mDaysFromLastEclosionIni As Single = 0, mDaysFromLastEclosion As Single = 0
    Private mDaysWithoutWaterIni As Single = 0, mDaysWithoutWater As Single = 0
    Private mFunctionResults(2) As Single
    Private mNumberOfStepsPerDay As Single = 0
    'Ajouté pour la matrice du modèle ?
    Private Model(4, 4) As Single

    'VARIABLES PARAMETERS INPUTS/OUTPUTS/RESULTS
    Private __z As String = "z"
    Private __TUp As String = "T"
    Private __Level As String = "Level"
    Private __StadeLarvaire As String = "StadeLarvaire"
    Private __DaysFromLastEclosion As String = "DaysFromLastEclosion"
    Private __DaysWithoutWater As String = "DaysWithoutWater"

    'PARAMETERS
    Private __z_parameter As Integer = 0
    Private __stadelarvaire_parameter As Integer = 1
    Private __DaysFromLastEclosion_parameter As Integer = 2
    Private __DaysWithoutWater_parameter As Integer = 3

    'INPUT/OUTPUT/RESULT
    Private __TUp_input As Integer = 0
    Private __Level_input As Integer = 1
    Private __StadeLarvaire_result As Integer = 0
    Private __DaysFromLastEclosion_result As Integer = 1
    Private __DaysWithoutWater_result As Integer = 2

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
        P = New Parameter(ParameterTypeEnum.NotDefined, __StadeLarvaire, "(-)")
        P.ParamValue = 1.0F
        MyBase.Parameters.Add(P)
        P = Nothing
        '2
        P = New Parameter(ParameterTypeEnum.NotDefined, __DaysFromLastEclosion, "(-)")
        P.ParamValue = 0.0F
        MyBase.Parameters.Add(P)
        P = Nothing
        '3
        P = New Parameter(ParameterTypeEnum.NotDefined, __DaysWithoutWater, "(-)")
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
        MyBase.Results.Add(New Result(New Parameter(ParameterTypeEnum.NotDefined, __StadeLarvaire, "(-)")))
        '1
        MyBase.Results.Add(New Result(New Parameter(ParameterTypeEnum.NotDefined, __DaysFromLastEclosion, "(-)")))
        '2
        MyBase.Results.Add(New Result(New Parameter(ParameterTypeEnum.NotDefined, __DaysWithoutWater, "(-)")))

        MyBase.InitIntegralOutputs()
    End Sub

    Public Property z As Single
        Get
            Return CSng(MyBase.Parameters(__z_parameter).ParamValue)
        End Get
        Set(value As Single)
            MyBase.Parameters(__z_parameter).ParamValue = value
        End Set
    End Property

    Public Property StadeLarvaire As Single
        Get
            Return CSng(MyBase.Parameters(__stadelarvaire_parameter).ParamValue)
        End Get
        Set(value As Single)
            MyBase.Parameters(__stadelarvaire_parameter).ParamValue = value
        End Set
    End Property

    Public Property DaysFromLastEclosion As Integer
        Get
            Return CInt(MyBase.Parameters(__DaysFromLastEclosion_parameter).ParamValue)
        End Get
        Set(value As Integer)
            MyBase.Parameters(__DaysFromLastEclosion_parameter).ParamValue = value
        End Set
    End Property

    Public Overrides Sub PrepareComputation()

        mNumberOfStepsPerDay = 86400.0F / CType(Application.Solver, SolverEuler1).dt

        mStadeLarvaire = mStadeLarvaireIni
        mDaysFromLastEclosion = mDaysFromLastEclosionIni
        mDaysWithoutWater = mDaysWithoutWaterIni

        'Row 0 : temperature associée
        'Row 1-4 : durée [jour] des stades L1 à L4
        Model = {{15.0F, 20.0F, 25.0F, 30.0F, 35.0F}, _
                {3.4F, 2.4F, 1.5F, 0.9F, 0.7F}, _
                {2.0F, 1.1F, 0.9F, 0.9F, 0.5F}, _
                {2.8F, 1.7F, 0.9F, 0.9F, 1.0F}, _
                {8.1F, 3.3F, 2.4F, 2.0F, 3.0F}}

    End Sub

    Public Overrides Sub ComputeOutput()

        'Evaluation du niveau et de la submersion
        Dim _Level As Single = CSng(MyBase.Inputs(__Level_input).Value)
        Dim _T As Single = CSng(MyBase.Inputs(__TUp_input).Value)

        Dim mStadeLarvaireResult() As Single
        If _Level >= Me.z Then

            mStadeLarvaireResult = Me.CalculateStadeLarvaire(mStadeLarvaire, _T, mDaysFromLastEclosion)
            mStadeLarvaire = mStadeLarvaireResult(0)
            mDaysFromLastEclosion = mStadeLarvaireResult(1)
            mDaysWithoutWater = 0
        Else
            'Si les larves sont privées d'eau, elles survivent quand meme quelques jours (on choisit 5 jours, pour etre du coté de la sécurité...) mais après, elles meurent.

            'Au début de l'utilisation du programme, certaines zones ne seront pas inondées. 
            'Ce n'est pas pour autant qu'il faut autoriser un développement pendant les 5 
            'premiers jours. => IDEE)donner a mDaysWithoutWater la valeur -1 comme valeur par 
            'defaut. Cette(valeur)n'aura plus jamais lieu une fois que le polygone évalué aura 
            'été inondé au moins une fois.
            If mDaysWithoutWater <> -1.0F Then
                mDaysWithoutWater += 1.0F / mNumberOfStepsPerDay

                If mDaysWithoutWater < 5.0F Then
                    mStadeLarvaireResult = Me.CalculateStadeLarvaire(mStadeLarvaire, _T, mDaysFromLastEclosion)
                    mStadeLarvaire = mStadeLarvaireResult(0)
                    mDaysFromLastEclosion = mStadeLarvaireResult(1)
                Else
                    mStadeLarvaire = 0.0F
                    mDaysFromLastEclosion += 1.0F / mNumberOfStepsPerDay
                End If
            Else
                mStadeLarvaire = 0.0F
                mDaysFromLastEclosion += 1.0F / mNumberOfStepsPerDay
            End If
        End If

        MyBase.IntegralCount += 1
        MyBase.IntegralOutputs(__StadeLarvaire_result) = mStadeLarvaire * MyBase.IntegralCount
        MyBase.IntegralOutputs(__DaysFromLastEclosion_result) = mDaysFromLastEclosion * MyBase.IntegralCount
        MyBase.IntegralOutputs(__DaysWithoutWater_result) = mDaysWithoutWater * MyBase.IntegralCount

    End Sub

    'MAX: T représente la température de la veille
    Function CalculateStadeLarvaire(ByVal stadeLarvaire As Single, ByVal T As Single, ByVal daysFromLastEclosion As Single) As Single()

        'Declaration des variables de sortie
        Dim result As Single()
        ReDim result(2)
        Dim state_today As Integer = CInt(Math.Truncate(stadeLarvaire))
        Dim percentage_today As Single = stadeLarvaire - state_today

        'catégories de temperature
        'Dim temp() As Single = {15.0F, 20.0F, 25.0F, 30.0F, 35.0F}

        'Durée stades vexans (établi à l'aide du code matlab "adaptation_albopictus_vexans.m")
        'Dim L1() As Single = {3.4F, 2.4F, 1.5F, 0.9F, 0.7F}
        'Dim L2() As Single = {2.0F, 1.1F, 0.9F, 0.9F, 0.5F}
        'Dim L3() As Single = {2.8F, 1.7F, 0.9F, 0.9F, 1.0F}
        'Dim L4() As Single = {8.1F, 3.3F, 2.4F, 2.0F, 3.0F}

        'Durée stades albopictus
        'Dim L1() As Double = {5.6, 3.0, 2.1, 1.4, 1.7}
        'Dim L2() As Double = {3.3, 1.4, 1.2, 1.3, 1.2}
        'Dim L3() As Double = {4.6, 2.1, 1.2, 1.4, 2.4}
        'Dim L4() As Double = {13.4, 4.1, 3.3, 3.0, 6.8}


        Dim noCol As Integer = 4
        'Dim Model(4, noCol) As Single

        'For i = 0 To noCol
        '    Model(0, i) = temp(i)
        '    Model(1, i) = L1(i)
        '    Model(2, i) = L2(i)
        '    Model(3, i) = L3(i)
        '    Model(4, i) = L4(i)
        'Next

        Dim i As Integer

        Dim percentage_incr As Single 'MAX: cette variable montre l'augmentation du pourcentage de la maturation de l'état qui avait lieu entre hier et aujourd'hui
        Dim time_new_state As Single 'MAX: dans le cas où un changement d'état a lieu entre hier et aujourd'hui, cette variable indique le temps qui s'est écoulé depuis que ce nouvel état a lieu.



        If T < Model(0, 0) Or T > Model(0, noCol) Then
            'Console.WriteLine("Temperature " & T & "°C out of range [ " & Model(0, 0) & ":" & Model(0, noCol) & " ]")
        Else

            For i = 0 To noCol - 1
                If T >= Model(0, i) And T < Model(0, i + 1) Then
                    'Compute a linear estimation of the require number of days to grow up
                    'then compute the actual proportion of growth since the simulation is at a daily scale
                    If state_today <> 0 Then
                        percentage_incr = CSng(1.0F / mNumberOfStepsPerDay / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i))))
                        If percentage_incr + percentage_today >= 1.0F Then
                            If state_today = 4 Then
                                time_new_state = 0
                                state_today = 0
                                percentage_today = 0
                            Else
                                time_new_state = (percentage_incr + percentage_today - 1) / percentage_incr
                                state_today += 1 ' this one gets out of range
                                percentage_today = time_new_state / mNumberOfStepsPerDay / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i)))
                                'If percentage_today >= 1.0F Then
                                '    Dim break As Integer = 0
                                '    While break = 0
                                '        state_today = state_today + 1
                                '        time_new_state = (percentage_today - 1) * (1 / percentage_today)
                                '        percentage_today = time_new_state / mNumberOfStepsPerDay / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i)))
                                '        If percentage_today < 1.0F Then
                                '            break = 1
                                '        End If
                                '    End While
                                'End If
                            End If
                        Else
                            time_new_state = 0
                            percentage_today += percentage_incr
                        End If
                    Else
                        If daysFromLastEclosion >= 15.0F Then
                            state_today = 1
                            percentage_today = CSng(1.0F / mNumberOfStepsPerDay / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i))))
                            daysFromLastEclosion = 0
                        End If
                    End If
                End If
            Next
        End If

        daysFromLastEclosion += 1.0F / mNumberOfStepsPerDay
        result(0) = state_today + percentage_today
        result(1) = daysFromLastEclosion

        Return result

    End Function

    Public Overrides Sub SaveStep()
        For i As Integer = 0 To MyBase.Results.Count - 1
            MyBase.Results(i).Add(MyBase.IntegralOutputs(i) / MyBase.IntegralCount)
            MyBase.IntegralOutputs(i) = 0
        Next
        MyBase.IntegralCount = 0
    End Sub

    Public Overrides Sub SaveFirstStep()

        MyBase.Results(__StadeLarvaire_result).Add(mStadeLarvaireIni)
        MyBase.Results(__DaysFromLastEclosion_result).Add(mDaysFromLastEclosionIni)
        MyBase.Results(__DaysWithoutWater_result).Add(mDaysWithoutWaterIni)

        MyBase.IntegralOutputs(__StadeLarvaire_result) = 0
        MyBase.IntegralOutputs(__DaysFromLastEclosion_result) = 0
        MyBase.IntegralOutputs(__DaysWithoutWater_result) = 0
        MyBase.IntegralCount = 0
    End Sub

    Public Overrides ReadOnly Property GetInitialConditions() As System.Collections.Generic.List(Of Single)
        Get
            Dim L As New List(Of Single)
            L.Add(CSng(Me.Parameters(__stadelarvaire_parameter).ParamValue))
            L.Add(CSng(Me.Parameters(__DaysFromLastEclosion_parameter).ParamValue))
            L.Add(CSng(Me.Parameters(__DaysWithoutWater_parameter).ParamValue))
            Return L
        End Get
    End Property

    Public Overrides Sub DefaultStart(ByVal Coeff As Single)
        mStadeLarvaireIni = CSng(Me.Parameters(__stadelarvaire_parameter).ParamValue)
        mDaysFromLastEclosionIni = CInt(Me.Parameters(__DaysFromLastEclosion_parameter).ParamValue)
        mDaysWithoutWaterIni = CInt(Me.Parameters(__DaysWithoutWater_parameter).ParamValue)
    End Sub

    Public Overrides Sub SetStateParameters()
        Me.Parameters(__stadelarvaire_parameter).ParamValue = mStadeLarvaireIni
        Me.Parameters(__DaysFromLastEclosion_parameter).ParamValue = mDaysFromLastEclosionIni
        Me.Parameters(__DaysWithoutWater_parameter).ParamValue = mDaysWithoutWaterIni
    End Sub

    Public Overrides Sub Hotstart(ByVal index As Integer, ByVal Coeff As Single, Optional ByVal UpdateRange As Single = 1000)
        'Débit initial constant dans le tronçon
        If Results(__StadeLarvaire_result).X.Length >= index + 1 Then mStadeLarvaireIni = Results(__StadeLarvaire_result).X(index)
        If Results(__DaysFromLastEclosion_result).X.Length >= index + 1 Then mDaysFromLastEclosionIni = CInt(Results(__DaysFromLastEclosion_result).X(index))
        If Results(__DaysWithoutWater_result).X.Length >= index + 1 Then mDaysWithoutWaterIni = CInt(Results(__DaysWithoutWater_result).X(index))
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

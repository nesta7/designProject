%nombre de jours nécessaires pour l'accomplissement 
%de chaque stade à différentes températures
%(développement de L1 à adulte)

%lines = temperatures. Columns = states
days_per_temp_and_state_albo=[5.6 3.3 4.6 13.4 8.7;
						3.0 1.4 2.1 4.1 4.1;
						2.1 1.2 1.2 3.3 2.7;
						1.4 1.3 1.4 3.0 1.9;
						1.7 1.2 2.4 6.8 1.7];

temperatures=[15 20 25 30 35];						
						
%nombre de jours nécessaires pour le développement						
days_tot_per_temp_albo=sum(days_per_temp_and_state_albo');

days_tot_per_temp_vex=[21.5 11.75 7.75 6 6];

figure(1)
plot(temperatures, days_tot_per_temp_albo);
hold on
plot(temperatures, days_tot_per_temp_vex);

prop_factor=(days_tot_per_temp_vex./days_tot_per_temp_albo)';

days_per_temp_and_state_vex=days_per_temp_and_state.*prop_factor
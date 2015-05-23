%petit script pour d�terminer les niveaux min et max du lac en utilisant les donnees d'archive

%En gros, on v�rifie dans les donn�es � disposition quelle �tait le niveau maximum atteint par l'eau et on d�cide de
%ne pas consid�rer les niveaux sup�rieurs.

tabl_file=fopen('Hauteur_Temp_with0.csv');
tabl = textscan(tabl_file, '%s %f %f', 'HeaderLines', 1, 'delimiter', ';');
hauteurs=tabl{2};

hauteur_min=min(hauteurs)
hauteur_max=max(hauteurs)
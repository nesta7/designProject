%petit script pour déterminer les niveaux min et max du lac en utilisant les donnees d'archive

%En gros, on vérifie dans les données à disposition quelle était le niveau maximum atteint par l'eau et on décide de
%ne pas considérer les niveaux supérieurs.

tabl_file=fopen('Hauteur_Temp_with0.csv');
tabl = textscan(tabl_file, '%s %f %f', 'HeaderLines', 1, 'delimiter', ';');
hauteurs=tabl{2};

hauteur_min=min(hauteurs)
hauteur_max=max(hauteurs)
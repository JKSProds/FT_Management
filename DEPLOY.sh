cd ~/FT_Management

git clone https://github.com/JKSProds/FT_Management.git

cp appsettings.json FT_Management/FT_Management/appsettings.json

cd FT_Management/FT_Management

docker build -t ftmanagement .

docker stop FT_Management
docker rm FT_Management

cd ..
cd ..

chmod -R 777 FT_Management

rm -r FT_Management

docker run -d -p 8082:80 --restart always --name FT_Management ftmanagement 

docker image prune -a -f


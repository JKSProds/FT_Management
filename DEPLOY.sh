cd ~/FT_Management

git clone https://github.com/JKSProds/FT_Management.git

cd FT_Management/FT_Management

docker build -t ftmanagement .

docker stop FT_Management
docker rm FT_Management

cd ..
cd ..

chmod -R 777 FT_Management

rm -r FT_Management

docker run -d -p 8080:80 --restart always --name FT_Management ftmanagement




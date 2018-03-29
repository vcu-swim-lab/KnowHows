if [ -d "Website" ]; then
  docker kill knowhows
  docker rm knowhows
  docker run -p 53222:53222 -v $PWD/data/:/dotnetapp/Website/data/:z -d --name knowhows knowhows
else
  echo "Run this script from the root directory of this project (i.e. Website folder should be present)"
fi

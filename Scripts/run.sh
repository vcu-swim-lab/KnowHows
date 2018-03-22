if [ -d "Website" ]; then
  cd Website
  docker kill knowhows
  docker rm knowhows
  docker run -p 53222:53222 -d --name knowhows knowhows
else
  echo "Run this script from the root directory of this project (i.e. Website folder should be present)"
fi

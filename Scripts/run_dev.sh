if [ -d "Website" ]; then
  cd Website
  docker kill knowhows_dev
  docker rm knowhows_dev
  docker run -p 53223:53223 -d --name knowhows_dev knowhows_dev
else
  echo "Run this script from the root directory of this project (i.e. Website folder should be present)"
fi

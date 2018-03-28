if [ -d "Website" ]; then
  cd Website
  docker build -t knowhows_dev .
else
  echo "Run this script from the root directory of this project (i.e. Website folder should be present)"
fi


if [ -d "Website" ]; then
  docker build -t knowhows .
else
  echo "Run this script from the root directory of this project (i.e. Website folder should be present)"
fi

